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


namespace VITacademics.UIControls
{
    public sealed partial class EnhancedTimetableControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {
        #region Fields and Properties

        private const int ARRAY_SIZE = 7;

        private DateTimeOffset[] _dates = new DateTimeOffset[ARRAY_SIZE];
        private DateTimeOffset _currentDate;
        private DatePickerFlyout _datePickerFlyout;
        private CalendarAwareStub _currentStub;
        private bool _contentReady;
        private CalendarAwareDayInfo _awareDayInfo;

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

        public List<string> EventMessages
        {
            get;
            set;
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
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CurrentDate"));
            }
        }

        #endregion

        #region Interface Implementations

        public event EventHandler<RequestEventArgs> ActionRequested;
        public event PropertyChangedEventHandler PropertyChanged;

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
                addFlyout.Hide();
                modifyFlyout.Hide();
                if (_datePickerFlyout != null)
                    _datePickerFlyout.Hide();
                eventMessageFlyout.Hide();

                List<PivotItem> pivotItems = new List<PivotItem>(ARRAY_SIZE);
                for (int i = 0; i < ARRAY_SIZE; i++)
                    pivotItems.Add(new PivotItem());
                _dates[0] = DateTimeOffset.Now;
                rootPivot.PivotItemLoading += Pivot_PivotItemLoading;
                rootPivot.ItemsSource = pivotItems;
                await CalendarManager.LoadCalendarAsync();
                ContentReady = true;

                // Restore last state if available
                if (lastState != null)
                {
                    JumpToDate((DateTimeOffset)lastState["selectedDate"]);
                }
            }
            catch { }
        }

        #endregion

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
            EventMessages.Add("LAB Term-End");
            EventMessages.Add("Class test");
            EventMessages.Add("Class cancelled");
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Daily Buzz");
#endif
            DataUpdated += DataUpdatedHandler;

            if (AppSettings.FirstRun == true)
                ShowPromptAsync();
        }

        #region Methods

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

        private async void DataUpdatedHandler()
        {
            if (AwareDayInfo != null && ContentReady == true)
                await CalendarManager.LoadRemindersAsync(AwareDayInfo);
        }

        private void Pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            int curIndex = sender.SelectedIndex;
            for (int i = curIndex + 1; i < curIndex + (ARRAY_SIZE - 1); i++)
            {
                _dates[i % ARRAY_SIZE] = _dates[curIndex].AddDays(i - curIndex);
            }
            int previousIndex = (curIndex + (ARRAY_SIZE - 1)) % ARRAY_SIZE;
            _dates[previousIndex] = _dates[curIndex].AddDays(-1);
            for (int i = 0; i < ARRAY_SIZE; i++)
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

        private void JumpToDate(DateTimeOffset requestedDate)
        {
            int index = (rootPivot.SelectedIndex + 1) % ARRAY_SIZE;
            _dates[index] = requestedDate;
            rootPivot.SelectedIndex = index;
        }

        #endregion

        #region Event handlers

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

        private void ItemRootGrid_Holding(object sender, HoldingRoutedEventArgs e)
        {
            CalendarAwareStub stub = (sender as FrameworkElement).DataContext as CalendarAwareStub;
            if (stub.ApptInfo == null)
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
                await CalendarManager.WriteAppointmentAsync(_currentStub, eventMessageFlyout.SelectedItem as string, TimeSpan.FromMinutes(15));
            _currentStub = null;
            eventMessageFlyout.SelectedItem = null;
        }

        private async void DeleteEventMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStub != null)
                await CalendarManager.RemoveAppointmentAsync(_currentStub);
            _currentStub = null;
        }

        private void List_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
                ActionRequested(this, new RequestEventArgs(typeof(CourseInfoControl),
                                        (e.ClickedItem as CalendarAwareStub).ContextCourse.ClassNumber.ToString()));
        }

        #endregion
        
    }
}
