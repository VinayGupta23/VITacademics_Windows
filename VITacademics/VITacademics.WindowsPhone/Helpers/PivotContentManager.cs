using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VITacademics.UIControls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace VITacademics.Helpers
{
    public class PivotContentManager
    {
        private enum PivotContentType
        {
            CourseView = 0,
            BasicTimetableView = 1,
            EnhancedTimetableView = 2
        }

        private readonly Pivot _pivot;
        private readonly Dictionary<string, Style> _stylesDictionary;

        private DateTime _curDate;
        private List<PivotItem> _pivotItems;
        private List<DateTime> _dates;
        private PivotContentType _currentContent;
        private Timetable _timetable;

        private void SetContentHandles(PivotContentType newContent)
        {
            if(_currentContent == PivotContentType.EnhancedTimetableView)
            {
                _timetable = null;
                _pivot.PivotItemLoading -= Pivot_PivotItemLoading;
            }

            _currentContent = newContent;

            if(newContent == PivotContentType.EnhancedTimetableView)
            {
                _pivot.PivotItemLoading += Pivot_PivotItemLoading;
            }
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

            if (_dates[curIndex].Date == DateTime.Now.Date)
                args.Item.Content = new CurrentDayControl();
            else
                args.Item.Content = new EnhancedDayControl();
            args.Item.DataContext = _timetable.GetDayInfo(_dates[curIndex]);
        }

        /// <summary>
        /// Constructs a wrapper that can manage content for the passed Pivot instance.
        /// </summary>
        /// <param name="pivot"></param>
        /// <remarks>
        /// If the passed pivot instance is manipulated or modified by the user, the current wrapper may become nonfunctional and must be reconstructed.
        /// </remarks>
        public PivotContentManager(Pivot pivot)
        {
            _pivot = pivot;
            _stylesDictionary = new Dictionary<string,Style>();
            foreach(var resource in pivot.Resources)
            {
                Style style = resource.Value as Style;
                if (style != null)
                    _stylesDictionary.Add(resource.Key as string, style);
            }
        }

        public void RefreshViewItems(Course course)
        {
            bool hasAttendance = true;
            _pivotItems = new List<PivotItem>(2);
            _pivotItems.Add(new PivotItem());
            _pivotItems[0].Header = "details";
            switch(course.CourseMode)
            {
                case "CBL":
                    _pivotItems[0].Style = _stylesDictionary["CBLCoursePivotItemStyle"];
                    break;
                case "PBL":
                    _pivotItems[0].Style = _stylesDictionary["PBLCoursePivotItemStyle"];
                    break;
                case "LBC":
                    _pivotItems[0].Style = _stylesDictionary["LBCCoursePivotItemStyle"];
                    break;
                case "PBC":
                    _pivotItems[0].Style = _stylesDictionary["PBCCoursePivotItemStyle"];
                    hasAttendance = false;
                    break;
            }
            if (hasAttendance == true)
            {
                _pivotItems.Add(new PivotItem());
                _pivotItems[1].Header = "attendance";
                _pivotItems[1].Content = new CourseAttendanceControl((course as LtpCourse).Attendance);
            }

            SetContentHandles(PivotContentType.CourseView);
            _pivot.DataContext = course;
            _pivot.ItemsSource = _pivotItems;
        }

        public void RefreshViewItems(Timetable timetable, bool enhancedView)
        {
            if (enhancedView == false)
            {
                _pivotItems = new List<PivotItem>(7);
                for (int i = 0; i < 7; i++)
                {
                    _pivotItems.Add(new PivotItem());
                    _pivotItems[i].Header = ((DayOfWeek)i).ToString().ToLower();
                    _pivotItems[i].Content = new BasicDayControl();
                    _pivotItems[i].DataContext = timetable[(DayOfWeek)i];
                }
                SetContentHandles(PivotContentType.BasicTimetableView);
                _pivot.ItemsSource = _pivotItems;
            }
            else
            {
                _pivotItems = new List<PivotItem>(5);
                _dates = new List<DateTime>(5);
                for (int i = 0; i < 5; i++)
                {
                    _pivotItems.Add(new PivotItem());
                    _dates.Add(new DateTime());
                }
                _dates[0] = DateTime.Now;
                SetContentHandles(PivotContentType.EnhancedTimetableView);
                _timetable = timetable;
                _pivot.ItemsSource = _pivotItems;
            }
        }

    }
}
