using Academics.DataModel;
using System;
using System.Collections.Generic;
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


namespace VITacademics.UIControls
{
    public sealed partial class EnhancedTimetableControl : UserControl, IProxiedControl
    {
        private Timetable _timetable;
        private DateTimeOffset[] _dates = new DateTimeOffset[5];
        public event EventHandler<RequestEventArgs> ActionRequested;

        public EnhancedTimetableControl()
        {
            this.InitializeComponent();
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

            PivotItem curItem = sender.Items[curIndex] as PivotItem;
            DayInfo dayInfo = _timetable.GetExactDayInfo(_dates[curIndex]);
            if (dayInfo.RegularClassDetails.Count > 0)
            {
                DateTimeOffset dateNow = DateTimeOffset.Now;
                if (_dates[curIndex].Day == dateNow.Day && _dates[curIndex].DayOfWeek == dateNow.DayOfWeek)
                    curItem.ContentTemplate = this.Resources["TodayDataTemplate"] as DataTemplate;
                else
                    curItem.ContentTemplate = this.Resources["RegularDayDataTemplate"] as DataTemplate;
            }
            else
                curItem.ContentTemplate = this.Resources["EmptyDayDataTemplate"] as DataTemplate;
            curItem.DataContext = dayInfo;
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
            state.Add("selectedDate", _dates[rootPivot.SelectedIndex]);
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
            _dates[0] = requestedDate;
            rootPivot.SelectedIndex = 0;
        }
    
    }
}
