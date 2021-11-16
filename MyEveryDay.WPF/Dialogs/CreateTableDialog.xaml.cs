using FzLib;
using ModernWpf.Controls;
using ModernWpf.FzExtension.CommonDialog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public interface ITableInfo
    {
        public int RowCount { get; }
        public IList<int> ColumnWidths { get; }
        public bool Border { get; }
    }
    public class IndexAndWidth : INotifyPropertyChanged
    {
        private int index;
        public int Index
        {
            get => index;
            set => this.SetValueAndNotify(ref index, value, nameof(Index));
        }
        private int width;

        public event PropertyChangedEventHandler PropertyChanged;

        public int Width
        {
            get => width;
            set
            {
                if (value > 500)
                {
                    value = 500;
                }
                if (value <= 10)
                {
                    value = 10;
                }
                this.SetValueAndNotify(ref width, value, nameof(Width));
            }

        }

    }
    public class CreateTableDialogViewModel : INotifyPropertyChanged, ITableInfo
    {
        public CreateTableDialogViewModel()
        {
            RowCount = 3;
            ColumnCount = 3;
        }
 
        private ObservableCollection<IndexAndWidth> columns;
        public ObservableCollection<IndexAndWidth> Columns
        {
            get => columns;
            set => this.SetValueAndNotify(ref columns, value, nameof(Columns));
        }

        private int? columnCount;

        public int? ColumnCount
        {
            get => columnCount;
            set
            {
                if (value.HasValue && value <= 0)
                {
                    value = 1;
                }
                Columns = value.HasValue == null ? null
                    : new ObservableCollection<IndexAndWidth>(
                    Enumerable.Range(1, value.Value)
                    .Select(p => new IndexAndWidth() { Index = p, Width = 120 }));
                this.SetValueAndNotify(ref columnCount, value, nameof(ColumnCount), nameof(CanOk));
            }
        }

        private int? rowCount;

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

        private bool border=true;
        public bool Border
        {
            get => border;
            set => this.SetValueAndNotify(ref border, value, nameof(Border));
        }


        public bool CanOk => RowCount.HasValue && ColumnCount.HasValue;

        public IList<int> ColumnWidths => Columns.Select(p => p.Width).ToList();
   
        int ITableInfo.RowCount => rowCount.Value;

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

        public ITableInfo Result =>
            ViewModel.RowCount.HasValue && ViewModel.ColumnCount.HasValue ?
       ViewModel : null;
    }
}