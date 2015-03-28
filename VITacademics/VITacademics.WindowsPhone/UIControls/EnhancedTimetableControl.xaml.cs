using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VITacademics.Managers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VITacademics.Helpers;


namespace VITacademics.UIControls
{
    public sealed partial class EnhancedTimetableControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {

        private Timetable _timetable;
        private DateTimeOffset[] _dates = new DateTimeOffset[5];
        public event EventHandler<RequestEventArgs> ActionRequested;
        public event PropertyChangedEventHandler PropertyChanged;
        private DateTimeOffset _currentDate;
        private DatePickerFlyout datePickerFlyout;

        public CalenderAwareDayInfo AwareDayInfo
        {
            get;
            set;
        }
        public DateTimeOffset CurrentDate
        {
            get { return _currentDate; }
            set
            {
                _currentDate = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentDate"));
            }
        }

        public EnhancedTimetableControl()
        {
            this.InitializeComponent();
            this.DataContext = this;

            datePickerFlyout = new DatePickerFlyout();
            datePickerFlyout.MinYear = new DateTimeOffset(DateTime.Now).AddYears(-5);
            datePickerFlyout.MaxYear = datePickerFlyout.MinYear.AddYears(10);
            datePickerFlyout.DatePicked += DatePickerFlyout_DatePicked;
        }

        private void DatePickerFlyout_DatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
        {
            if (CurrentDate != args.NewDate)
                JumpToDate(args.NewDate);
        }

        private void Pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            int curIndex = sender.SelectedIndex;
            for (int i = curIndex + 1; i < curIndex + 4; i++)
            {
                _dates[i % 5] = _dates[curIndex].AddDays(i - curIndex);
            }
            int previousIndex = (curIndex + 4) % 5;
            _dates[previousIndex] = _dates[curIndex].AddDays(-1);
            for (int i = 0; i < 5; i++)
            {
                (sender.Items[i] as PivotItem).Header = _dates[i].ToString("ddd dd");
            }

            CurrentDate = _dates[curIndex];
            AwareDayInfo = new CalenderAwareDayInfo(CurrentDate, _timetable.GetExactDayInfo(_dates[curIndex]));
            (sender.Items[curIndex] as PivotItem).DataContext = AwareDayInfo;
        }

        public void GenerateView(string parameter)
        {
            try
            {
                _timetable = Timetable.GetTimetable(UserManager.CurrentUser.Courses);
                List<PivotItem> pivotItems = new List<PivotItem>(5);
                for (int i = 0; i < 5; i++)
                    pivotItems.Add(new PivotItem());
                _dates[0] = DateTimeOffset.Now;
                rootPivot.PivotItemLoading += Pivot_PivotItemLoading;
                rootPivot.ItemsSource = pivotItems;
            }
            catch { }
        }

        public Dictionary<string, object> SaveState()
        {
            var state = new Dictionary<string, object>(1);
            state.Add("selectedDate", CurrentDate);
            return state;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {
            try
            {
                if(lastState != null)
                {
                    JumpToDate((DateTimeOffset)lastState["selectedDate"]);
                }
            }
            catch { }
        }

        private void JumpToDate(DateTimeOffset requestedDate)
        {
            int index = (rootPivot.SelectedIndex + 1)%5;
            _dates[index] = requestedDate;
            rootPivot.SelectedIndex = index;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            datePickerFlyout.Date = CurrentDate;
            datePickerFlyout.ShowAt(dateDisplayBlock);
        }

    }
}
