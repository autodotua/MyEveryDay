using FzLib;
using ModernWpf.Controls;
using ModernWpf.FzExtension.CommonDialog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class CreateTableDialogViewModel : INotifyPropertyChanged
    {
        private int? columnCount=3;

        public int? ColumnCount
        {
            get => columnCount;
            set
            {
                if (value.HasValue && value <= 0)
                {
                    value = 1;
                }
                this.SetValueAndNotify(ref columnCount, value, nameof(ColumnCount), nameof(CanOk));
            }
        }

        private int? rowCount=3;

        public int? RowCount
        {
            get => rowCount;
            set
            {
                if (value.HasValue && value <= 0)
                {
                    value = 1;
                }
                this.SetValueAndNotify(ref rowCount, value, nameof(RowCount), nameof(CanOk));
            }
        }

        public bool CanOk => RowCount.HasValue && ColumnCount.HasValue;

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// CreateTableDialog.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTableDialog : ContentDialog
    {
        public CreateTableDialogViewModel ViewModel { get; } = new CreateTableDialogViewModel();

        public CreateTableDialog()
        {
            DataContext = ViewModel;
            InitializeComponent();
        }

        public (int RowCount, int ColumnCount) Result =>
            ViewModel.RowCount.HasValue && ViewModel.ColumnCount.HasValue ?
            (ViewModel.RowCount.Value, ViewModel.ColumnCount.Value)
            : (0, 0);
    }
}