using Microsoft.WindowsAPICodePack.Dialogs.Controls;
using Microsoft.WindowsAPICodePack.FzExtension;
using ModernWpf.FzExtension.CommonDialog;
using MyEveryDay.WPF.Dialogs;
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
                await textArea.LoadDataAsync(e.Year, e.Month, e.Day);
            }
        }

        private async void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var date = dateSelector.CurrentDate;
            var dialog = new ExportDialog(date);
            await dialog.ShowAsync();
        }

        private void TemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TemplateWindow() { Owner = this };
            dialog.ShowDialog();
        }
    }
}
