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
using Windows.UI.ViewManagement;
using Windows.UI;
using Academics.ContentService;
using System.Threading.Tasks;
using System.ComponentModel;
using VITacademics.UIControls;
using Academics.DataModel;
using System.Runtime.CompilerServices;


namespace VITacademics
{

    public sealed partial class MainPage : Page, IManageable, INotifyPropertyChanged, IAppReturnControllable
    {

        private IProxiedControl proxiedControl;
        private object currentContentSource;
        private StatusBar _statusBar = StatusBar.GetForCurrentView();
        private MenuControl _menu = new MenuControl();        
        private bool _isMenuOpen;
        private string _titleText = null;

        public bool IsIdle
        {
            get { return !UserManager.IsBusy; }
        }
        private bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set
            {
                _isMenuOpen = value;
                NotifyPropertyChanged("TitleText");
            }
        }
        public string TitleText
        {
            get
            {
                if (_titleText == null || IsMenuOpen == true) return "VITacademics";
                else return _titleText;
            }
            set
            {
                _titleText = value;
                NotifyPropertyChanged();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            UserManager.PropertyChanged += UserManager_PropertyChanged;
            _menu.ActionRequested += ProxiedControl_ActionRequested;

            _statusBar.BackgroundColor = (Application.Current.Resources["AlternateDarkBrush"] as SolidColorBrush).Color;
            _statusBar.ForegroundColor = Colors.LightGray;
            _statusBar.ShowAsync();

        }

        void UserManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBusy")
                NotifyPropertyChanged("IsIdle");
            if(e.PropertyName == "CurrentUser" && UserManager.CurrentUser != null)
            {
                _menu.GenerateView(UserManager.CurrentUser.Courses);
                if (proxiedControl != null)
                {
                    if (currentContentSource as Course != null)
                    {
                        int classNo = (currentContentSource as Course).ClassNumber;
                        foreach (Course c in UserManager.CurrentUser.Courses)
                            if (c.ClassNumber == classNo)
                            {
                                currentContentSource = c;
                                break;
                            }
                    }
                    else if (currentContentSource as Timetable != null)
                        currentContentSource = Timetable.GetTimetable(UserManager.CurrentUser.Courses);
                    else
                        currentContentSource = UserManager.CurrentUser.Courses;

                    proxiedControl.GenerateView(currentContentSource);
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PageManager.RegisterPage(this);
        }

        #region IManageable Interface Implementation

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public async void LoadState(Dictionary<string, object> lastState)
        {
            if(UserManager.CurrentUser.CoursesMetadata == null)
            {
                bool fromCache = false;
                StatusCode status = await UserManager.LoadCacheAsync();
                if (status == StatusCode.Success)
                {
                    fromCache = true;
                }
                else
                {
                    status = await UserManager.RefreshFromServerAsync();
                    fromCache = false;
                }
                DisplayStatus(status, fromCache);
            }
            loadingScreenPresenter.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
        }

        #endregion

        #region INotifyPropertyChanged Interface Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Refresh Event Handler and Dependencies

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            _statusBar.ProgressIndicator.Text = "Refreshing...";
            _statusBar.ProgressIndicator.ProgressValue = null;
            _statusBar.ProgressIndicator.ShowAsync();

            StatusCode code = await UserManager.RefreshFromServerAsync();

            DisplayStatus(code, false);
        }

        private string GetTimeString(DateTimeOffset date)
        {
            date = date.ToLocalTime();
            DateTimeOffset today = DateTimeOffset.Now;
            if (date.Month == today.Month)
            {
                if (date.Day == today.Day)

                    return "at " + date.ToString("HH:mm");
                else
                    return "on " + date.ToString("ddd, HH:mm");
            }
            else
                return "on " + date.ToString("dd MMM, HH:mm");
        }
        private void DisplayStatus(StatusCode status, bool refreshedFromCache)
        {
            var metaData = UserManager.CurrentUser.CoursesMetadata;

            if (status == StatusCode.Success)
            {
                if (refreshedFromCache == false)
                    _statusBar.ProgressIndicator.Text = "Last refreshed " + GetTimeString(DateTimeOffset.Now);
                else
                    _statusBar.ProgressIndicator.Text = "Cache loaded, refreshed " + GetTimeString(metaData.RefreshedDate);
            }
            else
            {
                if (metaData == null)
                    _statusBar.ProgressIndicator.Text = "Unable to refresh, last tried at " + DateTimeOffset.Now.ToString("HH:mm");
                else
                    _statusBar.ProgressIndicator.Text = "Unable to refresh, last updated " + GetTimeString(metaData.RefreshedDate);
                StandardMessageDialogs.GetDialog(status).ShowAsync();
            }

            _statusBar.ProgressIndicator.ProgressValue = 0;
            _statusBar.ProgressIndicator.ShowAsync();
        }

        #endregion

        #region Navigation Request and Related Handlers

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(AboutPage), null, NavigationType.Default);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(SettingsPage), null, NavigationType.Default);
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMenuOpen)
                menuPresenter.Content = null;
            else
                menuPresenter.Content = _menu;
            IsMenuOpen = !IsMenuOpen;
        }

        private void ProxiedControl_ActionRequested(object sender, RequestEventArgs e)
        {
            if (e.TargetElement == typeof(CourseInfoControl))
            {
                proxiedControl = new CourseInfoControl();
                TitleText = "Course Details";
            }
            else if (e.TargetElement == typeof(BasicTimetableControl))
            {
                proxiedControl = new BasicTimetableControl();
                TitleText = "Timetable";
            }
            else if (e.TargetElement == typeof(EnhancedTimetableControl))
            {
                proxiedControl = new EnhancedTimetableControl();
                TitleText = "Daily Schedule";
            }
            else
            {
                proxiedControl = new UserOverviewControl();
                TitleText = "Overview";
            }

            proxiedControl.GenerateView(e.Parameter);
            currentContentSource = e.Parameter;
            proxiedControl.ActionRequested += ProxiedControl_ActionRequested;
            contentPresenter.Content = proxiedControl;

            if (sender as MenuControl != null)
                MenuButton_Click(null, null);
        }

        #endregion

        #region IAppReturnControllable Interface Implementation

        public bool AllowAppExit()
        {
            if (IsMenuOpen == true)
            {
                MenuButton_Click(null, null);
                return false;
            }
            else
                return true;
        }

        #endregion
    }
}
