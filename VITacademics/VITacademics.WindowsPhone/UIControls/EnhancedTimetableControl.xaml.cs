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
        private DatePickerFlyout _datePickerFlyout;
        private CalenderAwareInfoStub _currentStub;

        public List<string> EventMessages
        {
            get;
            set;
        }

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
            
            EventMessages = new List<string>();
            EventMessages.Add("Quiz I");
            EventMessages.Add("Quiz II");
            EventMessages.Add("Quiz III");
            EventMessages.Add("Assignment deadline");
            EventMessages.Add("Record submission");
            EventMessages.Add("LAB Mid-Term");
            EventMessages.Add("Class test");
            EventMessages.Add("Class cancelled");
        }


        private void DatePickerFlyout_DatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
        {
            if (CurrentDate != args.NewDate)
                JumpToDate(args.NewDate);
            sender.DatePicked -= DatePickerFlyout_DatePicked;
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
                string header;
                DateTimeOffset date = DateTimeOffset.Now.Date;
                if (_dates[i].Date == date)
                    header = "today";
                else if (_dates[i].AddDays(-1).Date == date)
                    header = "tomorrow";
                else
                    header = _dates[i].ToString("ddd dd");

                (sender.Items[i] as PivotItem).Header = header;
            }

            CurrentDate = _dates[curIndex];
            AwareDayInfo = new CalenderAwareDayInfo(CurrentDate, _timetable.GetExactDayInfo(_dates[curIndex]));
            (sender.Items[curIndex] as PivotItem).DataContext = AwareDayInfo;
            LoadAppointmentsAsync();
        }

        private async void LoadAppointmentsAsync()
        {
            await CalendarHelper.LoadCalendarAsync();
            foreach (CalenderAwareInfoStub stub in AwareDayInfo.RegularClassesInfo)
                await CalendarHelper.AssignAppointmentIfAvailableAsync(stub);
        }

        public void GenerateView(string parameter)
        {
            try
            {
                addFlyout.Hide();
                modifyFlyout.Hide();
                if (_datePickerFlyout != null)
                    _datePickerFlyout.Hide();
                eventMessageFlyout.Hide();
                
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
            int index = (rootPivot.SelectedIndex + 1) % 5;
            _dates[index] = requestedDate;
            rootPivot.SelectedIndex = index;
        }

        private void DateButton_Click(object sender, RoutedEventArgs e)
        {
            _datePickerFlyout = new DatePickerFlyout();
            _datePickerFlyout.Date = CurrentDate;
            _datePickerFlyout.DatePicked += DatePickerFlyout_DatePicked;
            _datePickerFlyout.ShowAt(dateDisplayButton);
        }

        private void ItemRootGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            CalenderAwareInfoStub stub = (sender as FrameworkElement).DataContext as CalenderAwareInfoStub;
            if(stub.AppointmentInfo == null)
            {
                addFlyout.ShowAt(sender as FrameworkElement);
            }
            else
            {
                modifyFlyout.ShowAt(sender as FrameworkElement);
            }
            _currentStub = stub;
        }

        private async void WriteEventMenuItem_Click(object sender, RoutedEventArgs e)
        {
            await eventMessageFlyout.ShowAtAsync(rootPivot);
            if (_currentStub != null && eventMessageFlyout.SelectedItem != null)
                await CalendarHelper.WriteAppointmentAsync(_currentStub, eventMessageFlyout.SelectedItem as string);
            _currentStub = null;
            eventMessageFlyout.SelectedItem = null;
        }

        private async void DeleteEventMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStub != null)
                await CalendarHelper.RemoveAppointmentAsync(_currentStub);
            _currentStub = null;
        }

        private void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(ActionRequested != null)
                ActionRequested(this, new RequestEventArgs(typeof(CourseInfoControl),
                                        (e.ClickedItem as CalenderAwareInfoStub).SessionHours.Parent.ClassNumber.ToString()));
        }

        private void ViewTodayButton_Click(object sender, RoutedEventArgs e)
        {
            JumpToDate(DateTimeOffset.Now);
        }

    }
}
