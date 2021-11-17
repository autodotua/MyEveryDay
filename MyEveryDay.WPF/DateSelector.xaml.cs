using FzLib;
using ModernWpf.Controls;
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

namespace MyEveryDay.WPF
{
    public class DateSelectorViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<int> years;
        public ObservableCollection<int> Years
        {
            get => years;
            set => this.SetValueAndNotify(ref years, value, nameof(Years));
        }
        private ObservableCollection<int> months;
        public ObservableCollection<int> Months
        {
            get => months;
            set => this.SetValueAndNotify(ref months, value, nameof(Months));
        }
        private ObservableCollection<int> days;
        public ObservableCollection<int> Days
        {
            get => days;
            set => this.SetValueAndNotify(ref days, value, nameof(Days));
        }
        private int? year;
        public int? Year
        {
            get => year;
            set => this.SetValueAndNotify(ref year, value, nameof(Year));
        }
        private int? month;
        public int? Month
        {
            get => month;
            set => this.SetValueAndNotify(ref month, value, nameof(Month));
        }
        private int? day;

        public event PropertyChangedEventHandler PropertyChanged;

        public int? Day
        {
            get => day;
            set => this.SetValueAndNotify(ref day, value, nameof(Day));
        }

    }
    public partial class DateSelector : UserControl, INotifyPropertyChanged
    {
        public DateSelectorViewModel ViewModel { get; set; } = new DateSelectorViewModel();
        public DateSelector()
        {
            InitializeComponent();
            DataContext = ViewModel;
        }

        private async void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var today = DateTime.Today;
            switch (e.PropertyName)
            {
                case nameof(ViewModel.Year):
                    {
                        if (ViewModel.Year == null)
                        {
                            ViewModel.Months.Clear();
                            ViewModel.Month = null;
                            break;
                        }
                        ViewModel.Months = new ObservableCollection<int>(await RecordService.GetMonths(ViewModel.Year.Value));
                        if (ViewModel.Year == today.Year && !ViewModel.Months.Contains(today.Month))
                        {
                            InsertIntoDates(ViewModel.Months, today.Month);
                        }
                        if (ViewModel.Months.Count > 0)
                        {
                            ViewModel.Month = ViewModel.Months[0];
                        }
                    }
                    break;
                case nameof(ViewModel.Month):
                    {
                        if (ViewModel.Month == null)
                        {
                            ViewModel.Days.Clear();
                            ViewModel.Day = null;
                            break;
                        }
                        ViewModel.Days = new ObservableCollection<int>(await RecordService.GetDays(ViewModel.Year.Value, ViewModel.Month.Value));
                        if (ViewModel.Year == today.Year && ViewModel.Month == today.Month && !ViewModel.Days.Contains(today.Day))
                        {
                            InsertIntoDates(ViewModel.Days, today.Day);
                        }
                        if (ViewModel.Days.Count > 0)
                        {
                            ViewModel.Day = ViewModel.Days[0];
                        }
                    }
                    break;
                case nameof(ViewModel.Day):
                    if (ViewModel.Day == null)
                    {
                        SelectedDateChanged?.Invoke(this, SelectedDateChangedEventArgs.FromNull());
                    }
                    else
                    {
                        SelectedDateChanged?.Invoke(this, SelectedDateChangedEventArgs.FromDate(ViewModel.Year.Value, ViewModel.Month.Value, ViewModel.Day.Value));
                    }
                    break;
                default:
                    break;
            }
        }

        public (int? Year, int? Month, int? Day) CurrentDate => (ViewModel.Year, ViewModel.Month, ViewModel.Day);
        public async Task InitializeAsync()
        {
            var today = DateTime.Today;
            ViewModel.Years = new ObservableCollection<int>(await RecordService.GetYears());
            if (!ViewModel.Years.Contains(today.Year))
            {
                InsertIntoDates(ViewModel.Years, today.Year);
            }
            ViewModel.Year = today.Year;

            ViewModel.Months = new ObservableCollection<int>(await RecordService.GetMonths(today.Year));
            if (!ViewModel.Months.Contains(today.Month))
            {
                InsertIntoDates(ViewModel.Months, today.Month);
            }
            ViewModel.Month = today.Month;

            ViewModel.Days = new ObservableCollection<int>(await RecordService.GetDays(today.Year, today.Month));
            if (!ViewModel.Days.Contains(today.Day))
            {
                InsertIntoDates(ViewModel.Days, today.Day);
            }

            ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.Day = today.Day;
        }

        private void InsertIntoDates(ObservableCollection<int> list, int date)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] > date)
                {
                    list.Insert(i, date);
                    return;
                }
            }
            list.Add(date);
        }

        private void AddYearButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = FlyoutService.GetFlyout(sender as Button) as MenuFlyout;
            for (int i = DateTime.Today.Year - 10; i <= DateTime.Today.Year + 10; i++)
            {
                if (!ViewModel.Years.Contains(i))
                {
                    var item = new MenuItem() { Header = i + "年" };
                    int year = i;
                    item.Click += (s, e) => InsertIntoDates(ViewModel.Years, year);
                    menu.Items.Add(item);
                }
            }
        }

        private void AddMonthButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = FlyoutService.GetFlyout(sender as Button) as MenuFlyout;
            menu.Items.Clear();
            if (ViewModel.Year == null)
            {
                return;
            }
            for (int i = 1; i <= 12; i++)
            {
                if (!ViewModel.Months.Contains(i))
                {
                    var item = new MenuItem() { Header = i + "月" };
                    int month = i;
                    item.Click += (s, e) => InsertIntoDates(ViewModel.Months, month);
                    menu.Items.Add(item);
                }
            }
        }
        private void AddDayButton_Click(object sender, RoutedEventArgs e)
        {
            var menu = FlyoutService.GetFlyout(sender as Button) as MenuFlyout;
            menu.Items.Clear();
            if (ViewModel.Year == null || ViewModel.Month == null)
            {
                return;
            }
            int count = DateTime.DaysInMonth(ViewModel.Year.Value, ViewModel.Month.Value);
            for (int i = 1; i <= count; i++)
            {
                if (!ViewModel.Days.Contains(i))
                {
                    var item = new MenuItem() { Header = i + "日" };
                    int day = i;
                    item.Click += (s, e) => InsertIntoDates(ViewModel.Days, day);
                    menu.Items.Add(item);
                }
            }
        }
        public event EventHandler<SelectedDateChangedEventArgs> SelectedDateChanged;
        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class SelectedDateChangedEventArgs
    {
        private SelectedDateChangedEventArgs()
        {

        }
        private SelectedDateChangedEventArgs(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
            IsNull = false;
        }

        public static SelectedDateChangedEventArgs FromDate(int year, int month, int day)
        {
            return new SelectedDateChangedEventArgs(year, month, day);
        }
        public static SelectedDateChangedEventArgs FromNull()
        {
            return new SelectedDateChangedEventArgs();
        }

        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        public bool IsNull { get; } = true;
    }
}
