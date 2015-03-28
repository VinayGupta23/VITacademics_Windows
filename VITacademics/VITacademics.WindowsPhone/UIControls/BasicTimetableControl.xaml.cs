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
    public sealed partial class BasicTimetableControl : UserControl, IProxiedControl
    {
        public event EventHandler<RequestEventArgs> ActionRequested;

        public BasicTimetableControl()
        {
            this.InitializeComponent();
        }

        public void GenerateView(string parameter)
        {
            try
            {
                Timetable timetable = Timetable.GetTimetable(UserManager.CurrentUser.Courses);
                int j = 0;
                List<PivotItem> pivotItems = new List<PivotItem>(7);
                for (int i = 0; i < 7; i++)
                {
                    var daySchedule = timetable[(DayOfWeek)i];
                    if (daySchedule.Count != 0)
                    {
                        pivotItems.Add(new PivotItem());
                        pivotItems[j].Header = ((DayOfWeek)i).ToString().ToLower();
                        pivotItems[j].DataContext = daySchedule;
                        j++;
                    }
                }
                rootPivot.ItemsSource = pivotItems;
            }
            catch { }
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(ActionRequested != null)
            {
                ActionRequested(this, new RequestEventArgs(typeof(CourseInfoControl), (e.ClickedItem as ClassHours).Parent.ClassNumber.ToString()));
            }
        }

        public Dictionary<string, object> SaveState()
        {
            var state = new Dictionary<string,object>(1);
            state.Add("currentIndex", rootPivot.SelectedIndex);
            return state;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {
            try
            {
                if (lastState != null)
                    rootPivot.SelectedIndex = (int)lastState["currentIndex"];
            }
            catch { }
        }
    }
}
