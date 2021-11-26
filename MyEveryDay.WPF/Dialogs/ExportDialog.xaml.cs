#define WORD_TEST

using FzLib;
using Microsoft.WindowsAPICodePack.FzExtension;
using ModernWpf.Controls;
using MyEveryDay.Service;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace MyEveryDay.WPF.Dialogs
{
    public class ExportDialogViewModel : INotifyPropertyChanged
    {
        public ExportDialogViewModel()
        {
        }

        private int range = 0;
        /// <summary>
        /// 导出范围
        /// </summary>
        /// <remarks>
        /// 0日，1月，2年，3全部
        /// </remarks>
        public int Range
        {
            get => range;
            set => this.SetValueAndNotify(ref range, value, nameof(Range));
        }
        /// <summary>
        /// 按日月年分文件
        /// </summary>
        /// <remarks>
        /// 0不分，1按日，2按月，3按年
        /// </remarks>
        public int Split { get; set; }
        public string[] Formats => new[] { "RTF","Word" };
#if DEBUG && WORD_TEST
        private string format = "Word";
#else
        private string format = "RTF";
#endif
        public string Format
        {
            get => format;
            set => this.SetValueAndNotify(ref format, value, nameof(Format));
        }
        private string message;
        public string Message
        {
            get => message;
            set => this.SetValueAndNotify(ref message, value, nameof(Message));
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// ExportDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ExportDialog : ContentDialog
    {
        private readonly (int? Year, int? Month, int? Day) date;

        public ExportDialogViewModel ViewModel { get; } = new ExportDialogViewModel();

        public ExportDialog((int? Year, int? Month, int? Day) date)
        {
            DataContext = ViewModel;
            InitializeComponent();
            this.date = date;
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            args.Cancel = true;
            IsEnabled = false;
            if (ViewModel.Split == 0)
            {
                FileFilterCollection filter = new FileFilterCollection();
                switch (ViewModel.Format)
                {
                    case "RTF":
                        filter.Add("RTF 文档", "rtf");
                        break;
                    case "Word":
                        filter.Add("Word 文档", "docx");
                        break;
                    default:
                        break;
                }
                var path = filter
                    .CreateSaveFileDialog()
                    .SetDefault("日记")
                    .GetFilePath();
                if (path != null)
                {
                    try
                    {
                        await ExportToSingleFileAsync(path);
                    }
                    catch (Exception ex)
                    {
                        ViewModel.Message = ex.Message;
                        IsEnabled = true;
                        return;
                    }
                }
            }
            else
            {

            }
            Hide();
        }

        private async Task ExportToSingleFileAsync(string path)
        {
            CheckDateSelection();
            switch (ViewModel.Format)
            {
                case "RTF":
                    await ExportService.ExportRtfAsync(path,
                                                       ViewModel.Range,
                                                       date);
                    break;
                case "Word":
                    await ExportService.ExportWordAsync(path,
                                                       ViewModel.Range,
                                                       date);
                    break;
                default:
                    break;
            }
        }


        private void CheckDateSelection()
        {
            if (ViewModel.Range < 3 && !date.Year.HasValue)
            {
                throw new Exception("没有选择年份");
            }
            if (ViewModel.Range < 2 && !date.Month.HasValue)
            {
                throw new Exception("没有选择月份");
            }
            if (ViewModel.Range < 1 && !date.Day.HasValue)
            {
                throw new Exception("没有选择日");
            }
        }
    }
}