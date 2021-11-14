using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyEveryDay.WPF
{
    /// <summary>
    /// TextArea.xaml 的交互逻辑
    /// </summary>
    public partial class TextArea : UserControl
    {
        private (int year, int month, int day)? date = null;
        private bool needSave = false;
        private Timer timer;
        public TextArea()
        {
            InitializeComponent();
        }

        public async Task SaveAsync()
        {
            if (!date.HasValue)
            {
                return;
            }
            string text = null;
            string rtf = null;
            Dispatcher.Invoke(() =>
            {
                var range = new TextRange(txt.Document.ContentStart, txt.Document.ContentEnd);
                text = range.Text;
                MemoryStream ms = new MemoryStream();
                range.Save(ms, DataFormats.Rtf);
                ms.Close();
                rtf = Encoding.Default.GetString(ms.ToArray());
            });
            await RecordService.SaveAsync(date.Value.year, date.Value.month, date.Value.day, rtf, text);
        }


        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
            needSave = true;
        }

        public async Task DisableAsync()
        {
            needSave = false;
            txt.TextChanged -= txt_TextChanged;
            await SaveAsync();
            date = null;
            IsEnabled = false;
            txt.Document.Blocks.Clear();
        }
        private TextRange GetAllRange(RichTextBox rtb)
        {
       return     new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        }
        public async Task LoadData(int year, int month, int day)
        {
            txt.TextChanged -= txt_TextChanged;
            await SaveAsync();
            IsEnabled = true;
            txt.Document.Blocks.Clear();
            date = (year, month, day);
            try
            {
                var rtf = await RecordService.GetRichText(year, month, day);
                var range = GetAllRange(txt);
                MemoryStream ms = new MemoryStream(Encoding.Default.GetBytes(rtf));
                range.Load(ms, DataFormats.Rtf);
                txt.TextChanged += txt_TextChanged;
            }
            catch (KeyNotFoundException)
            {
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            timer = new Timer(new TimerCallback(async t =>
            {
                if (needSave)
                {
                    await SaveAsync();
                }
            }), null, 10000, 10000);
        }

        private async void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            await SaveAsync();
        }

        private void txt_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.V:
                    e.Handled = true;
                    Paste();
                    break;
                case Key.Q:
                    CreateTable(5, 5);
                    break;
            }
        }

        private void CreateTable(int column,int row)
        {
            var tab = new Table();
            var gridLenghtConvertor = new GridLengthConverter();
            for(int i=0;i<column;i++)
            {
                tab.Columns.Add(new TableColumn() {  Width = GridLength .Auto});
            }
          
            tab.RowGroups.Add(new TableRowGroup());

            for (int i = 0; i < row; i++)
            {
                tab.RowGroups[0].Rows.Add(new TableRow());
                var tabRow = tab.RowGroups[0].Rows[i];

                tabRow.Cells.Add(new TableCell(new Paragraph(new Run("R"+(i+1)))) { TextAlignment = TextAlignment.Center });
                for (int j=1;j<column;j++)
                {
                    tabRow.Cells.Add(new TableCell(new Paragraph(new Run(i==0?("C"+(j+1)): ""))) { TextAlignment = TextAlignment.Center });
                }
            }
            RichTextBox temp = new RichTextBox();
            temp.Document.Blocks.Add(tab);
            using var ms = new MemoryStream();
            GetAllRange(temp).Save(ms, DataFormats.Rtf);
            ms.Seek(0, SeekOrigin.Begin);
            txt.Selection.Load(ms, DataFormats.Rtf);
            txt.Selection.ApplyPropertyValue(ForegroundProperty, Foreground);
            txt.CaretPosition = txt.Selection.End;
        }
        private void Paste()
        {
            string format = null;
            if (Clipboard.ContainsData(DataFormats.Rtf))
            {
                format = DataFormats.Rtf;
            }
            else if (Clipboard.ContainsData(DataFormats.Text))
            {
                format = DataFormats.Text;
            }
            if (format != null)
            {
                using var ms = new MemoryStream(Encoding.Default.GetBytes(Clipboard.GetData(format) as string));
                txt.Selection.Load(ms, format);
                txt.Selection.ApplyPropertyValue(ForegroundProperty, Foreground);
                txt.CaretPosition = txt.Selection.End;
            }
        }

   
    }
}
