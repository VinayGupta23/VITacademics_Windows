using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using VITacademics.Helpers;
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
    public sealed partial class UserOverviewControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {

        public event EventHandler<RequestEventArgs> ActionRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        private int _totalCredits;
        private List<LtpCourse> _courseList;
        private List<NonLtpCourse> _nltpCourseList;

        public int TotalCredits
        {
            get { return _totalCredits; }
            private set
            {
                _totalCredits = value;
                NotifyPropertyChanged();
            }
        }
        public List<LtpCourse> CourseList
        {
            get { return _courseList; }
            private set
            {
                _courseList = value;
                NotifyPropertyChanged();
            }
        }
        public List<NonLtpCourse> NltpCourseList
        {
            get { return _nltpCourseList; }
            private set
            {
                _nltpCourseList = value;
                NotifyPropertyChanged();
            }
        }

        public UserOverviewControl()
        {
            this.InitializeComponent();
            this.DataContext = this;

            GoogleAnalytics.EasyTracker.GetTracker().SendView("Overview");
        }

        public void GenerateView(string parameter)
        {
            TotalCredits = 0;
            var courseList = new List<LtpCourse>();
            var nltpCourseList = new List<NonLtpCourse>();
            foreach (Course course in UserManager.CurrentUser.Courses)
            {
                LtpCourse c = course as LtpCourse;
                if(c != null)
                {
                    TotalCredits += int.Parse(c.Ltpc.Substring(3));
                    courseList.Add(c);
                }
                else
                {
                    NonLtpCourse nc = course as NonLtpCourse;
                    nltpCourseList.Add(nc);
                    TotalCredits += int.Parse(nc.Credits);
                }
            }
            CourseList = courseList;
            NltpCourseList = nltpCourseList;
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {
        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
                ActionRequested(this, new RequestEventArgs(typeof(CourseInfoControl), (e.ClickedItem as Course).ClassNumber.ToString()));
        }

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
