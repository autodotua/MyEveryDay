using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
            var range = new TextRange(txt.Document.ContentStart, txt.Document.ContentEnd);
            var text = range.Text;
            MemoryStream ms = new MemoryStream();
            range.Save(ms, DataFormats.Rtf);
            ms.Close();
            string rtf = Encoding.Default.GetString(ms.ToArray());
            await RecordService.SaveAsync(date.Value.year, date.Value.month, date.Value.day, rtf, text);
        }

        private void txt_TextChanged(object sender, TextChangedEventArgs e)
        {
        }

        public async Task DisableAsync()
        {
            txt.TextChanged -= txt_TextChanged;
            await SaveAsync();
            date = null;
            IsEnabled = false;
            txt.Document.Blocks.Clear();
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
                var range = new TextRange(txt.Document.ContentStart, txt.Document.ContentEnd);
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
    }
}
