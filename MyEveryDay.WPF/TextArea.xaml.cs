using FzLib;
using FzLib.WPF;
using MyEveryDay.WPF.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Imaging;
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
using System.Windows.Interop;
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

    public partial class TextArea : UserControl
    {
        private (int year, int month, int day)? date = null;
        private bool needSave = false;
        private Timer timer;
        private bool updatingSelection = false;
        public TextArea()
        {
            DataContext = ViewModel;
            InitializeComponent();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            Dispatcher.ShutdownFinished += (s, e) => SaveAsync().Wait();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (updatingSelection)
            {
                return;
            }
            switch (e.PropertyName)
            {
                case nameof(ViewModel.FontFamily):
                    txt.Selection.ApplyPropertyValue(FontFamilyProperty, ViewModel.FontFamily);
                    break;
                case nameof(ViewModel.FontSize):
                    txt.Selection.ApplyPropertyValue(FontSizeProperty, ViewModel.FontSize);
                    break;
                case nameof(ViewModel.Bold):
                    txt.Selection.ApplyPropertyValue(FontWeightProperty, ViewModel.Bold ? FontWeights.Bold : FontWeights.Normal);
                    break;
                case nameof(ViewModel.Italic):
                    txt.Selection.ApplyPropertyValue(FontStyleProperty, ViewModel.Italic ? FontStyles.Italic : FontStyles.Normal);
                    break;
                case nameof(ViewModel.Underline):
                    txt.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, ViewModel.Underline ? TextDecorations.Underline : new TextDecorationCollection());
                    break;
                default:
                    break;
            }

        }

        public TextAreaViewModel ViewModel { get; } = new TextAreaViewModel();
        public async Task DisableAsync()
        {
            needSave = false;
            txt.TextChanged -= TextChanged;
            await SaveAsync();
            date = null;
            IsEnabled = false;
            txt.Document.Blocks.Clear();
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



        private TextRange GetAllRange(RichTextBox rtb)
        {
            return new TextRange(rtb.Document.ContentStart, rtb.Document.ContentEnd);
        }

        private bool Paste()
        {
            string format = null;
            MemoryStream ms = null;
            if (Clipboard.ContainsData(DataFormats.Rtf))
            {
                format = DataFormats.Rtf;
                ms = new MemoryStream(Encoding.Default.GetBytes(Clipboard.GetData(format) as string));
            }
            else if (Clipboard.ContainsData(DataFormats.Text))
            {
                format = DataFormats.Text;
                ms = new MemoryStream(Encoding.Default.GetBytes(Clipboard.GetText()));
            }
            else if (Clipboard.ContainsData(DataFormats.Bitmap))
            {
                return false;
                //try
                //{
                //    format = DataFormats.Bitmap;
                //    var bitmap = Clipboard.GetImage().ToBitmap();
                //    ms = new MemoryStream();
                //    bitmap.Save(ms, ImageFormat.Png);
                //    ms.Seek(0, SeekOrigin.Begin);
                //}
                //catch (Exception ex)
                //{
                //    return;
                //}
            }
            if (format != null)
            {
                txt.Selection.Load(ms, format);
                txt.Selection.ApplyPropertyValue(ForegroundProperty, Foreground);
                txt.CaretPosition = txt.Selection.End;
            }
            ms?.Dispose();
            return true;
        }

        private void PasteButton_Click(object sender, RoutedEventArgs e)
        {
            Paste();
        }

        private void TextPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }
            switch (e.Key)
            {
                case Key.V:
                    if (Paste())
                    {

                        e.Handled = true;
                    }
                    break;

            }
        }

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            updatingSelection = true;
            if (txt.Selection.GetPropertyValue(FontFamilyProperty) is FontFamily font)
            {
                ViewModel.FontFamily = font;
            }
            if (txt.Selection.GetPropertyValue(FontSizeProperty) is double size)
            {
                ViewModel.FontSize = size;
            }
            if (txt.Selection.GetPropertyValue(FontWeightProperty) is FontWeight fontWeight)
            {
                ViewModel.Bold = fontWeight > FontWeights.Normal;
            }
            if (txt.Selection.GetPropertyValue(FontStyleProperty) is FontStyle fs)
            {
                ViewModel.Italic = fs == FontStyles.Italic;
            }
            if (txt.Selection.GetPropertyValue(Inline.TextDecorationsProperty) is TextDecorationCollection td)
            {
                ViewModel.Underline = td.Any(p => p.Location == TextDecorationLocation.Underline);
            }
            updatingSelection = false;
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

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            needSave = true;
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

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {

            CopyOrCut(false);
        }

        private void CutButton_Click(object sender, RoutedEventArgs e)
        {
            CopyOrCut(true);
        }

        private void CopyOrCut(bool cut)
        {
            var selection = txt.Selection;
            if (selection.IsEmpty)
            {
                return;
            }
            DataObject data = new DataObject();
            data.SetText(selection.Text);
            MemoryStream ms = new MemoryStream();
            selection.Save(ms, DataFormats.Rtf);
            ms.Close();
            data.SetData(DataFormats.Rtf, Encoding.Default.GetString(ms.ToArray()));
            Clipboard.SetDataObject(data, true);
            if (cut)
            {
                txt.Focus();
                System.Windows.Forms.SendKeys.SendWait("{BS}");
            }
        }

        private void AlignButton_Click(object sender, RoutedEventArgs e)
        {
            int type = int.Parse((sender as Button).Tag as string);
            switch (type)
            {
                case 0:
                    txt.Selection.ApplyPropertyValue(Block.TextAlignmentProperty, TextAlignment.Center);
                    break;
                case -1:
                    txt.Selection.ApplyPropertyValue(Block.TextAlignmentProperty, TextAlignment.Left);
                    break;
                case 1:
                    txt.Selection.ApplyPropertyValue(Block.TextAlignmentProperty, TextAlignment.Right);
                    break;
                default:
                    break;
            }
        }

        private void ClearFormatButton_Click(object sender, RoutedEventArgs e)
        {
            txt.Selection.ClearAllProperties();
        }


    }

    public class TextAreaViewModel : INotifyPropertyChanged
    {
        private double fontSize;

        private FontFamily selectedFont;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICollection<FontFamily> Fonts { get; }
                                    = System.Windows.Media.Fonts.SystemFontFamilies
            .OrderBy(p => p.FamilyNames.Keys.Contains(FontDisplayConverter.LocalLanguage))
            .ToList();
        public double FontSize
        {
            get => fontSize;
            set
            {
                if (value < 1)
                {
                    value = 1;
                }
                if (value > 144)
                {
                    value = 144;
                }
                this.SetValueAndNotify(ref fontSize, value, nameof(FontSize));
            }
        }
        public FontFamily FontFamily
        {
            get => selectedFont;
            set => this.SetValueAndNotify(ref selectedFont, value, nameof(FontFamily));
        }

        private bool bold;
        public bool Bold
        {
            get => bold;
            set => this.SetValueAndNotify(ref bold, value, nameof(Bold));
        }
        private bool italic;
        public bool Italic
        {
            get => italic;
            set => this.SetValueAndNotify(ref italic, value, nameof(Italic));
        }
        private bool underline;
        public bool Underline
        {
            get => underline;
            set => this.SetValueAndNotify(ref underline, value, nameof(Underline));
        }


    }
}