using FzLib;
using Microsoft.WindowsAPICodePack.FzExtension;
using ModernWpf.Controls;
using ModernWpf.FzExtension.CommonDialog;
using MyEveryDay.Service;
using NPOI.XWPF.UserModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        public string[] Formats => new[] { "RTF" };
        private string format = "RTF";
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
            switch (ViewModel.Format)
            {
                case "RTF":
                    await ExportRtfAsync(path);
                    break;
                default:
                    break;
            }
        }

        private async Task ExportRtfAsync(string path)
        {
            CheckDateSelection();
            await ExportService.ExportRtfAsync(path,
                                               ViewModel.Range,
                                               date,
                                               Config.Instance.YearTitle,
                                               Config.Instance.MonthTitle,
                                               Config.Instance.DayTitle);
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