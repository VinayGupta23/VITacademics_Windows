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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

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
            MenuItems.Add(new MenuItem("overview", "a summary of marks and attendance, today's schedule"));
            MenuItems.Add(new MenuItem("timetable", "your regular schedule of classes"));
            MenuItems.Add(new MenuItem("daily buzz", "the semester's activity embedded into the timetable"));

            this.DataContext = this;
        }

        public event EventHandler<RequestEventArgs> ActionRequested;

        public void GenerateView(object parameter)
        {
            Courses = parameter as IEnumerable<Course>;
        }

        private void CourseList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
                ActionRequested(this, new RequestEventArgs(typeof(CourseInfoControl), e.ClickedItem));
        }

        private void MenuList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
            {
                MenuItem item = e.ClickedItem as MenuItem;
                if (item != null)
                {
                    if (item == MenuItems[0])
                        ActionRequested(this, new RequestEventArgs(typeof(UserOverviewControl), UserManager.CurrentUser.Courses));
                    else if (item == MenuItems[1])
                        ActionRequested(this, new RequestEventArgs(typeof(BasicTimetableControl), Timetable.GetTimetable(UserManager.CurrentUser.Courses)));
                    else
                        ActionRequested(this, new RequestEventArgs(typeof(EnhancedTimetableControl), Timetable.GetTimetable(UserManager.CurrentUser.Courses)));
                }
            }
        }

    }
}
