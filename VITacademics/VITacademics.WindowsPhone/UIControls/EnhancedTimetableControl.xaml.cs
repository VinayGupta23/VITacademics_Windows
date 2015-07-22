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
using Windows.UI.Popups;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Windows.UI.Input;


namespace VITacademics.UIControls
{
    public sealed partial class EnhancedTimetableControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {

        #region Helper Class

        internal class DummyCalendarInfoStub : CalendarAwareStub
        {
            private readonly TimeSpan _startTime;
            private readonly TimeSpan _endTime;

            public override TimeSpan StartTime
            {
                get { return _startTime; }
            }

            public override TimeSpan EndTime
            {
                get { return _endTime; }
            }

            public DummyCalendarInfoStub(Course course, DateTimeOffset date, TimeSpan start, TimeSpan end)
                : base(date, course, null)
            {
                _startTime = start;
                _endTime = end;
            }
        }

        #endregion

        #region Fields and Properties

        private const int BUFFER_SIZE = 7;

        private DateTimeOffset[] _dates = new DateTimeOffset[BUFFER_SIZE];
        private DateTimeOffset _currentDate;
        private CalendarAwareStub _currentStub;
        private CalendarAwareDayInfo _awareDayInfo;
        private bool _contentReady;
        private bool _reminderSetupVisible;

        private DatePickerFlyout _datePickerFlyout;
        private ListPickerFlyout _listPickerFlyout;

        public delegate void DataChanged();
        private DataChanged DataUpdated;

        private bool ContentReady
        {
            get { return _contentReady; }
            set
            {
                _contentReady = value;
                DataUpdated();
            }
        }

        public CalendarAwareDayInfo AwareDayInfo
        {
            get { return _awareDayInfo; }
            set
            {
                _awareDayInfo = value;
                DataUpdated();
            }
        }
        public DateTimeOffset CurrentDate
        {
            get { return _currentDate; }
            private set
            {
                _currentDate = value;
                NotifyPropertyChanged();
            }
        }
        public CalendarAwareStub CurrentStub
        {
            get { return _currentStub; }
            set
            {
                _currentStub = value;
                NotifyPropertyChanged();
            }
        }
        public bool ReminderSetupVisible
        {
            get { return _reminderSetupVisible; }
            set
            {
                _reminderSetupVisible = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public EnhancedTimetableControl()
        {
            this.InitializeComponent();
            this.DataContext = this;

            DataUpdated += DataUpdatedHandler;
            CurrentStub = null;
            ReminderSetupVisible = false;

#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Daily Buzz");
#endif

            if (AppSettings.FirstRun == true)
                ShowPromptAsync();
        }

        #region Interface Implementations

        public event EventHandler<RequestEventArgs> ActionRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public string DisplayTitle
        {
            get { return "Daily Buzz"; }
        }

        public Dictionary<string, object> SaveState()
        {
            var state = new Dictionary<string, object>(1);
            state.Add("selectedDate", CurrentDate);
            return state;
        }

        public async void LoadView(string parameter, Dictionary<string, object> lastState = null)
        {
            try
            {
                addMenuFlyout.Hide();
                modifyMenuFlyout.Hide();
                if (_datePickerFlyout != null)
                    _datePickerFlyout.Hide();
                if (_listPickerFlyout != null)
                    _listPickerFlyout.Hide();

                List<PivotItem> pivotItems = new List<PivotItem>(BUFFER_SIZE);
                for (int i = 0; i < BUFFER_SIZE; i++)
                    pivotItems.Add(new PivotItem());
                _dates[0] = DateTimeOffset.Now;
                rootPivot.PivotItemLoading += Pivot_PivotItemLoading;
                rootPivot.ItemsSource = pivotItems;
                await CalendarManager.LoadCalendarAsync();
                ContentReady = true;

                if (lastState != null)
                {
                    JumpToDate((DateTimeOffset)lastState["selectedDate"]);
                }
            }
            catch { }
        }

        #endregion
        
        #region First Run Pop-up Display

        private async void ShowPromptAsync()
        {
            MessageDialog dialog = new MessageDialog("Well yes! You can now set reminders for your quizzes, assignments and a lot more.\n\nTap on 'more help' to learn how.", "Like to set reminders?");
            UICommand cmd1 = new UICommand("more help", DialogCommandHandler);
            UICommand cmd2 = new UICommand("dismiss", DialogCommandHandler);
            dialog.Commands.Add(cmd1);
            dialog.Commands.Add(cmd2);
            await dialog.ShowAsync();
        }

        private void DialogCommandHandler(IUICommand command)
        {
            if (command.Label == "more help")
                ActionRequested(this, new RequestEventArgs(typeof(HelpPage), null));
            AppSettings.FirstRun = false;
        }

        #endregion

        private async void DataUpdatedHandler()
        {
            if (AwareDayInfo != null && ContentReady == true)
                await CalendarManager.LoadRemindersAsync(AwareDayInfo);
        }

        private void Pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            int curIndex = sender.SelectedIndex;
            for (int i = curIndex + 1; i < curIndex + (BUFFER_SIZE - 1); i++)
            {
                _dates[i % BUFFER_SIZE] = _dates[curIndex].AddDays(i - curIndex);
            }
            int previousIndex = (curIndex + (BUFFER_SIZE - 1)) % BUFFER_SIZE;
            _dates[previousIndex] = _dates[curIndex].AddDays(-1);
            for (int i = 0; i < BUFFER_SIZE; i++)
            {
                string header;
                DateTimeOffset date = DateTimeOffset.Now.Date;
                if (_dates[i].Date == date)
                    header = "today";
                else if (_dates[i].AddDays(-1).Date == date)
                    header = "tomorrow";
                else if (_dates[i].AddDays(1).Date == date)
                    header = "yesterday";
                else
                    header = _dates[i].ToString("ddd dd").ToLower();

                (sender.Items[i] as PivotItem).Header = header;
            }

            CurrentDate = _dates[curIndex];
            AwareDayInfo = new CalendarAwareDayInfo(CurrentDate);
            (sender.Items[curIndex] as PivotItem).DataContext = AwareDayInfo;
        }

        #region Date Selection Methods

        private void JumpToDate(DateTimeOffset requestedDate)
        {
            int index = (rootPivot.SelectedIndex + 1) % BUFFER_SIZE;
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

        private void DatePickerFlyout_DatePicked(DatePickerFlyout sender, DatePickedEventArgs args)
        {
            if (CurrentDate != args.NewDate)
                JumpToDate(args.NewDate);
            sender.DatePicked -= DatePickerFlyout_DatePicked;
        }

        #endregion

        #region Reminder Menu and Flyout Handles

        private void ItemRootBorder_Holding(object sender, HoldingRoutedEventArgs e)
        {
            if (e.HoldingState != HoldingState.Started)
                return;

            Border source = sender as Border;
            CalendarAwareStub stub = source.DataContext as CalendarAwareStub;
            if (stub.ApptInfo == null)
                addMenuFlyout.ShowAt(source.Child as FrameworkElement);
            else
                modifyMenuFlyout.ShowAt(source.Child as FrameworkElement);
            CurrentStub = stub;
        }

        private async void ManualEventAddButton_Click(object sender, RoutedEventArgs e)
        {
            _listPickerFlyout = new ListPickerFlyout();
            _listPickerFlyout.ItemsSource = UserManager.CurrentUser.Courses;
            _listPickerFlyout.DisplayMemberPath = "Title";
            await _listPickerFlyout.ShowAtAsync(sender as FrameworkElement);
            if (_listPickerFlyout.SelectedIndex < 0)
                return;

            Course course = _listPickerFlyout.SelectedItem as Course;

            DateTimeOffset dateNow = CurrentDate.Date.AddHours(DateTimeOffset.Now.TimeOfDay.Hours + 1);
            DummyCalendarInfoStub dummyStub = new DummyCalendarInfoStub(course, dateNow, new TimeSpan(dateNow.Hour, 0, 0), new TimeSpan(dateNow.Hour + 1, 0, 0));
            CurrentStub = dummyStub;
            ReminderSetupVisible = true;
            rootPivot.Visibility = Visibility.Collapsed;
        }

        private void AddMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ReminderSetupVisible = true;
            rootPivot.Visibility = Visibility.Collapsed;
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ReminderSetupVisible = true;
            rootPivot.Visibility = Visibility.Collapsed;
        }

        private async void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await CalendarManager.RemoveAppointmentAsync(CurrentStub.ApptInfo.LocalId);
                if (CurrentStub is RegularInfoStub)
                    CurrentStub.ApptInfo = null;
                else
                    AwareDayInfo.RegularClassesInfo.Remove(CurrentStub);
            }
            catch { }
            finally
            {
                CurrentStub = null;
            }
        }

        private async void SetReminderButton_Click(object sender, RoutedEventArgs e)
        {
            ReminderControl reminderControl = (reminderContentControl.ContentTemplateRoot as FrameworkElement).FindName("reminderControl") as ReminderControl;
            if (reminderControl.Agenda == "")
            {
                await new MessageDialog("Please select or type an agenda for the reminder.", "Missing Input").ShowAsync();
                return;
            }
            else
            {
                if (CurrentStub.ApptInfo != null)
                    await CalendarManager.ModifyAppointmentAsync(CurrentStub.ApptInfo.LocalId, reminderControl.Agenda,
                                                                 reminderControl.ContextDate, reminderControl.StartTime,
                                                                 reminderControl.Duration, reminderControl.Reminder);
                else
                    await CalendarManager.WriteAppointmentAsync(CurrentStub.ContextCourse, reminderControl.Agenda,
                                                                reminderControl.ContextDate, reminderControl.StartTime,
                                                                reminderControl.Duration, reminderControl.Reminder);
            }

            ReminderSetupVisible = false;
            CurrentStub = null;
            rootPivot.Visibility = Windows.UI.Xaml.Visibility.Visible;

            AwareDayInfo = new CalendarAwareDayInfo(CurrentDate);
            (rootPivot.Items[rootPivot.SelectedIndex] as PivotItem).DataContext = AwareDayInfo;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            ReminderSetupVisible = false;
            CurrentStub = null;
            rootPivot.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        #endregion

        private void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
                ActionRequested(this, new RequestEventArgs(typeof(CourseInfoControl),
                                        (e.ClickedItem as CalendarAwareStub).ContextCourse.ClassNumber.ToString()));
        }

    }
}
