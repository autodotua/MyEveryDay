using Microsoft.WindowsAPICodePack.Dialogs.Controls;
using Microsoft.WindowsAPICodePack.FzExtension;
using System;
using System.Collections.Generic;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await dateSelector.InitializeAsync();
        }

        private async void dateSelector_SelectedDateChanged(object sender, SelectedDateChangedEventArgs e)
        {
            if (e.IsNull)
            {
                await textArea.DisableAsync();
            }
            else
            {
                await textArea.LoadData(e.Year, e.Month, e.Day);
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FileFilterCollection().Add("RTF 文档", "rtf")
                             .CreateOpenFileDialog();
            var combobox = new CommonFileDialogComboBox("导出范围");
            combobox.Items.Add(new CommonFileDialogComboBoxItem("当前日"));
            combobox.Items.Add(new CommonFileDialogComboBoxItem("当前月"));
            combobox.Items.Add(new CommonFileDialogComboBoxItem("当前年"));
            combobox.Items.Add(new CommonFileDialogComboBoxItem("全部"));
            combobox.SelectedIndex = 0;
            dialog.Controls.Add(combobox);
            string path = dialog.GetFilePath();
            if (path != null)
            {
                switch (dialog.SelectedFileTypeIndex)
                {
                    case 1:
                        switch (combobox.SelectedIndex)
                        {
                            case 0:
                                {
                                    using FileStream fs = new FileStream(path, FileMode.Create);
                                    var date = dateSelector.CurrentDate.Value;
                                    var bytes = Encoding.Default.GetBytes(await RecordService.GetRichText(date.Year, date.Month, date.Day));
                                    fs.Write(bytes, 0, bytes.Length);
                                    fs.Close();
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
