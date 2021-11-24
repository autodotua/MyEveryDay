using FzLib;
using Microsoft.WindowsAPICodePack.Dialogs.Controls;
using Microsoft.WindowsAPICodePack.FzExtension;
using ModernWpf.FzExtension.CommonDialog;
using MyEveryDay.Model;
using MyEveryDay.WPF.Dialogs;
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

namespace MyEveryDay.WPF
{
    public class TemplateWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private int dateTitleSelectedIndex;
        public int DateTitleSelectedIndex
        {
            get => dateTitleSelectedIndex;
            set
            {

                this.SetValueAndNotify(ref dateTitleSelectedIndex, value, nameof(DateTitleSelectedIndex));
                if (value >= 0)
                {
                    TextTemplateSelectedItem = null;
                }
            }
        }
        private Template textTemplateSelectedItem;
        public Template TextTemplateSelectedItem
        {
            get => textTemplateSelectedItem;
            set
            {
                this.SetValueAndNotify(ref textTemplateSelectedItem, value, nameof(TextTemplateSelectedItem));
                if (value != null)
                {
                    DateTitleSelectedIndex = -1;
                }
            }
        }
        private ObservableCollection<Template> textTemplates;
        public ObservableCollection<Template> TextTemplates
        {
            get => textTemplates;
            set => this.SetValueAndNotify(ref textTemplates, value, nameof(TextTemplates));
        }
        public async Task InitializeAsync()
        {
            TextTemplates = new ObservableCollection<Template>(await TemplateService.GetTextTemplatesAsync());
        }
    }
    public partial class TemplateWindow : Window
    {
        public TemplateWindowViewModel ViewModel { get; } = new TemplateWindowViewModel();
        public TemplateWindow()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        private async void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            throw new NotImplementedException();
            if (e.RemovedItems.Count > 0)
            {
                var r = (sender as ListView).Items.IndexOf(e.AddedItems[0]);
                if (r >= 0)
                {

                }
            }
            if (e.AddedItems.Count > 0)
            {
                var a = (sender as ListView).Items.IndexOf(e.AddedItems[0]);
                if (a >= 0)
                {
                    switch (a)
                    {
                        case 0:
                            textArea.LoadRtf(await TemplateService.GetDayTitleAsync());
                            break;
                    }
                }
            }
        }
    }
}
