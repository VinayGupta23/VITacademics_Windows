using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed partial class UserOverviewControl : UserControl, IProxiedControl
    {

        public event EventHandler<RequestEventArgs> ActionRequested;

        public int TotalCredits
        {
            get;
            private set;
        }
        public List<LtpCourse> CourseList
        {
            get;
            private set;
        }
        public List<NonLtpCourse> NltpCourseList
        {
            get;
            private set;
        }

        public UserOverviewControl()
        {
            this.InitializeComponent();
        }

        public void GenerateView(string parameter)
        {
            CourseList = new List<LtpCourse>();
            NltpCourseList = new List<NonLtpCourse>();
            foreach (Course course in UserManager.CurrentUser.Courses)
            {
                LtpCourse c = course as LtpCourse;
                if(c != null)
                {
                    TotalCredits += int.Parse(c.Ltpc.Substring(3));
                    CourseList.Add(c);
                }
                else
                {
                    NonLtpCourse nc = course as NonLtpCourse;
                    NltpCourseList.Add(nc);
                    TotalCredits += int.Parse(nc.Credits);
                }
            }

            this.DataContext = this;
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

    }
}
