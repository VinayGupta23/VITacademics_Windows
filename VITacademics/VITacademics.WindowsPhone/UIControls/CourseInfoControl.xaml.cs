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
    public sealed partial class CourseInfoControl : UserControl, IProxiedControl
    {
        public event EventHandler<RequestEventArgs> ActionRequested;

        public CourseInfoControl()
        {
            this.InitializeComponent();
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Course Details");
#endif
        }

        public void GenerateView(string parameter)
        {
            ContentPresenter contentPresenter = new ContentPresenter();
            try
            {
                Course course =
                    UserManager.CurrentUser.Courses.Single<Course>((Course c) => string.Equals(c.ClassNumber.ToString(), parameter));

                string courseType = course.CourseMode;
                if (courseType == "CBL")
                    contentPresenter.ContentTemplate = this.Resources["CBLPivotTemplate"] as DataTemplate;
                else if (courseType == "PBL")
                    contentPresenter.ContentTemplate = this.Resources["PBLPivotTemplate"] as DataTemplate;
                else if (courseType == "LBC")
                    contentPresenter.ContentTemplate = this.Resources["LBCPivotTemplate"] as DataTemplate;
                else if (courseType == "PBC")
                    contentPresenter.ContentTemplate = this.Resources["PBCPivotTemplate"] as DataTemplate;
                else if (courseType == "RBL")
                    contentPresenter.ContentTemplate = this.Resources["RBLPivotTemplate"] as DataTemplate;
                else
                    contentPresenter.ContentTemplate = null;
                contentPresenter.Content = course;
                this.Content = contentPresenter;
            }
            catch
            {
                contentPresenter.ContentTemplate = this.Resources["CourseNotFoundTemplate"] as DataTemplate;
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
