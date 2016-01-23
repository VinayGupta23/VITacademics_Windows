using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private int _totalCredits;
        private ReadOnlyCollection<Course> _courseList;

        public int TotalCredits
        {
            get { return _totalCredits; }
            private set
            {
                _totalCredits = value;
                NotifyPropertyChanged();
            }
        }
        public ReadOnlyCollection<Course> CourseList
        {
            get { return _courseList; }
            private set
            {
                _courseList = value;
                NotifyPropertyChanged();
            }
        }

        public UserOverviewControl()
        {
            this.InitializeComponent();
            this.DataContext = this;
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Overview");
#endif
        }

        #region IProxiedControl interface implementation

        public event EventHandler<RequestEventArgs> ActionRequested;
        public string DisplayTitle
        {
            get { return "Overview"; }
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public void LoadView(string parameter, Dictionary<string, object> lastState = null)
        {
            CourseList = UserManager.CurrentUser.Courses;
            TotalCredits = UserManager.CurrentUser.CoursesMetadata.TotalCredits;
        }

        #endregion

        #region Property Notification Interface implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName]string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
                ActionRequested(this, new RequestEventArgs(typeof(CourseInfoControl), (e.ClickedItem as Course).ClassNumber.ToString()));
        }

        
    }
}
