using Academics.DataModel;
using System;
using System.Collections.Generic;
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


namespace VITacademics.UIControls
{
    public sealed partial class MenuControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {

        public class MenuItem
        {
            public string Header
            {
                get;
                private set;
            }
            public string SubHeader
            {
                get;
                private set;
            }

            public MenuItem(string header, string subHeader)
            {
                Header = header;
                SubHeader = subHeader;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private IEnumerable<Course> _courses;

        public IEnumerable<Course> Courses
        {
            get
            {
                return _courses;
            }
            private set
            {
                _courses = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("Courses"));
            }
        }
        public List<MenuItem> MenuItems
        {
            get;
            private set;
        }

        public MenuControl()
        {
            this.InitializeComponent();

            MenuItems = new List<MenuItem>();
            MenuItems.Add(new MenuItem("overview", "a summary of marks and attendance"));
            MenuItems.Add(new MenuItem("timetable", "your regular schedule of classes"));
            MenuItems.Add(new MenuItem("daily buzz", "your schedule, reminders and attendance in one place"));

            this.DataContext = this;
        }

        public event EventHandler<RequestEventArgs> ActionRequested;

        public void GenerateView(string parameter)
        {
            Courses = UserManager.CurrentUser.Courses;
        }

        private void CourseList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
                ActionRequested(this, new RequestEventArgs(typeof(CourseInfoControl), (e.ClickedItem as Course).ClassNumber.ToString()));
        }

        private void MenuList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
            {
                MenuItem item = e.ClickedItem as MenuItem;
                if (item != null)
                {
                    if (item == MenuItems[0])
                        ActionRequested(this, new RequestEventArgs(typeof(UserOverviewControl), null));
                    else if (item == MenuItems[1])
                        ActionRequested(this, new RequestEventArgs(typeof(BasicTimetableControl), null));
                    else
                        ActionRequested(this, new RequestEventArgs(typeof(EnhancedTimetableControl), null));
                }
            }
        }


        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {

        }
    }
}
