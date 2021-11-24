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
                    ArticleTemplateSelectedItem = null;
                }
            }
        }
        private Template articleTemplateSelectedItem;
        public Template ArticleTemplateSelectedItem
        {
            get => articleTemplateSelectedItem;
            set
            {
                this.SetValueAndNotify(ref articleTemplateSelectedItem, value, nameof(ArticleTemplateSelectedItem));
                if (value != null)
                {
                    DateTitleSelectedIndex = -1;
                }
            }
        }
        private ObservableCollection<Template> articleTemplates;
        public ObservableCollection<Template> ArticleTemplates
        {
            get => articleTemplates;
            set => this.SetValueAndNotify(ref articleTemplates, value, nameof(ArticleTemplates));
        }
        public async Task InitializeAsync()
        {
            ArticleTemplates = new ObservableCollection<Template>(await TemplateService.GetArticleTemplatesAsync());
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

        private async void ArticleTemplateListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }
            if (e.RemovedItems.Count > 0)
            {
                var template = e.RemovedItems[0] as Template;
                if (!ViewModel.ArticleTemplates.Contains(template))//被删除
                {
                    return;
                }
                template.RichText = textArea.GetRtf();
                await TemplateService.UpdateArticleTemplateAsync(template.Id, template.RichText);
            }
            if (e.AddedItems.Count > 0)
            {
                var template = e.AddedItems[0] as Template;

                textArea.LoadRtf(template.RichText);

            }
        }
        private async void DateTitleListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized)
            {
                return;
            }
            if (e.RemovedItems.Count > 0)
            {
                var r = (sender as ListView).Items.IndexOf(e.RemovedItems[0]);
                if (r >= 0)
                {
                    switch (r)
                    {
                        case 0:
                            await TemplateService.UpdateDayTitle(textArea.GetRtf());
                            break;
                        case 1:
                            await TemplateService.UpdateMonthTitle(textArea.GetRtf());
                            break;
                        case 2:
                            await TemplateService.UpdateYearTitle(textArea.GetRtf());
                            break;
                    }
                }
            }
            if (e.AddedItems.Count > 0)
            {
                var a = (sender as ListView).Items.IndexOf(e.AddedItems[0]);
                switch (a)
                {
                    case 0:
                        textArea.LoadRtf(await TemplateService.GetDayTitleAsync());
                        break;
                    case 1:
                        textArea.LoadRtf(await TemplateService.GetMonthTitleAsync());
                        break;
                    case 2:
                        textArea.LoadRtf(await TemplateService.GetYearTitleAsync());
                        break;
                }
            }
        }

        private async void AddTemplateButton_Click(object sender, RoutedEventArgs e)
        {
            var name = await CommonDialog.ShowInputDialogAsync("请输入模板名");
            if (string.IsNullOrEmpty(name))
            {
                await CommonDialog.ShowErrorDialogAsync("模板名不可为空");
                return;
            }
            if (ViewModel.ArticleTemplates.Any(p => p.Name == name))
            {
                await CommonDialog.ShowErrorDialogAsync("该名字的模板已存在");
                return;
            }
            var template = await TemplateService.AddArticleTemplateAsync(name);
            ViewModel.ArticleTemplates.Add(template);
            ViewModel.ArticleTemplateSelectedItem = template;
        }




        private void ListViewItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //尝试重写样式来重写ContextMenu，但是会报错：
            //Unable to cast object of type 'System.Windows.Controls.MenuItem' to type 'System.Windows.Controls.Button'.
            ContextMenu menu = new ContextMenu();
            menu.PlacementTarget = sender as UIElement;
            var menuItem = new MenuItem()
            {
                Header = "删除",
                Icon = new ModernWpf.Controls.SymbolIcon(ModernWpf.Controls.Symbol.Delete)
            };
            menuItem.Click += async (sender, e) =>
             {
                 var template = (sender as FrameworkElement).DataContext as Template;
                 await TemplateService.DeleteArticleTemplateAsync(template.Id);
                 ViewModel.ArticleTemplates.Remove(template);
             };
            menu.Items.Add(menuItem);
            menu.IsOpen = true;
        }
    }
}
