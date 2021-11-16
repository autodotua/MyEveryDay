using FzLib;
using MyEveryDay.WPF.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MyEveryDay.WPF
{
    public class FontDisplayConverter : IValueConverter
    {
        public static readonly XmlLanguage LocalLanguage = XmlLanguage.GetLanguage(CultureInfo.CurrentUICulture.Name);
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is FontFamily))
            {
                return null;
            }

            var font = value as FontFamily;
            if (font.FamilyNames.Keys.Contains(LocalLanguage))
            {
                return font.FamilyNames[LocalLanguage];
            }
            return font.Source;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TextAreaViewModel : INotifyPropertyChanged
    {
        public ICollection<FontFamily> Fonts { get; } 
            = System.Windows.Media.Fonts.SystemFontFamilies
            .OrderBy(p=>p.FamilyNames.Keys.Contains(FontDisplayConverter.LocalLanguage))
            .ToList();
        private FontFamily selectedFont;

        public event PropertyChangedEventHandler PropertyChanged;

        public FontFamily SelectedFont
        {
            get => selectedFont;
            set => this.SetValueAndNotify(ref selectedFont, value, nameof(SelectedFont));
        }

    }
    public partial class TextArea : UserControl
    {
        private (int year, int month, int day)? date = null;
        private bool needSave = false;
        private Timer timer;
        public TextAreaViewModel ViewModel { get; } = new TextAreaViewModel();
        public TextArea()
        {
            DataContext = ViewModel;
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

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            needSave = true;
        }

        public async Task DisableAsync()
        {
            needSave = false;
            txt.TextChanged -= TextChanged;
            await SaveAsync();
            date = null;
            IsEnabled = false;
            txt.Document.Blocks.Clear();
        }

        private TextRange GetAllRange(RichTextBox rtb)
        {
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        }

        public async Task LoadData(int year, int month, int day)
        {
            txt.TextChanged -= TextChanged;
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
                txt.TextChanged += TextChanged;
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

        private void PreviewKeyDown(object sender, KeyEventArgs e)
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

            }
        }

        private void CreateTable(int rowCount, IList<int> columnWidths, bool border)
        {
            var tab = new Table();
            if (border)
            {
                tab.BorderBrush = Brushes.Gray;
                tab.BorderThickness = new Thickness(1);
            }
            var gridLenghtConvertor = new GridLengthConverter();
            foreach (var width in columnWidths)
            {
                tab.Columns.Add(new TableColumn() { Width = new GridLength(width) });
            }

            tab.RowGroups.Add(new TableRowGroup());

            for (int i = 0; i < rowCount; i++)
            {
                tab.RowGroups[0].Rows.Add(new TableRow());
                var tabRow = tab.RowGroups[0].Rows[i];

                tabRow.Cells.Add(new TableCell(new Paragraph(new Run(i == 0 ? "" : ("R" + (i + 1))))) { TextAlignment = TextAlignment.Center });
                for (int j = 1; j < columnWidths.Count; j++)
                {
                    tabRow.Cells.Add(new TableCell(new Paragraph(new Run(i == 0 ? ("C" + (j + 1)) : ""))) { TextAlignment = TextAlignment.Center });
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

        private async void TableButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CreateTableDialog();
            if (await dialog.ShowAsync() == ModernWpf.Controls.ContentDialogResult.Primary)
            {
                var table = dialog.Result;
                if (table != null)
                {

                    CreateTable(table.RowCount, table.ColumnWidths, table.Border);
                }
            }
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }
        private bool updatingFont = false;
        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            var font = txt.Selection.GetPropertyValue(FontFamilyProperty) as FontFamily;
            updatingFont = true;
            ViewModel.SelectedFont = font;
            updatingFont = false;
        }

        private void Font_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!updatingFont)
            {
                txt.Selection.ApplyPropertyValue(FontFamilyProperty, ViewModel.SelectedFont);
            }
        }
    }
}