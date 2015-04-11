using Academics.ContentService;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VITacademics.Helpers;
using VITacademics.Managers;
using VITacademics.UIControls;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace VITacademics
{

    public sealed partial class MainPage : Page, IManageable, INotifyPropertyChanged, IAppReturnControllable
    {

        private StatusBar _statusBar = StatusBar.GetForCurrentView();
        private MenuControl _menu;
        private ControlManager _contentControlManager;
        private bool _isMenuOpen;
        private bool _isIdle;
        private bool _isContentAvailable;
        private string _titleText = null;
        private bool _isCached = false;

        public bool IsIdle
        {
            get { return _isIdle; }
            private set
            {
                _isIdle = value;
                NotifyPropertyChanged();
            }
        }
        private bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set
            {
                _isMenuOpen = value;
                NotifyPropertyChanged("TitleText");
                NotifyPropertyChanged("CanGoBack");
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
        public Visibility CanGoBack
        {
            get
            {
                if (_contentControlManager != null && _contentControlManager.CanGoBack == true && IsMenuOpen == false)
                    return Windows.UI.Xaml.Visibility.Visible;
                else
                    return Windows.UI.Xaml.Visibility.Collapsed;
            }
        }
        public bool IsContentAvailable
        {
            get { return _isContentAvailable; }
            set
            {
                _isContentAvailable = value;
                NotifyPropertyChanged();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            _menu = new MenuControl();
            _contentControlManager = new ControlManager(ProxiedControl_ActionRequested);

            _statusBar.BackgroundColor = (Application.Current.Resources["AlternateDarkBrush"] as SolidColorBrush).Color;
            _statusBar.ForegroundColor = Colors.LightGray;
            _statusBar.ShowAsync();
        }

        void UserManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBusy")
                this.IsIdle = !UserManager.IsBusy;
            else if (e.PropertyName == "IsContentReady")
                this.IsContentAvailable = UserManager.IsContentReady;
            else if (e.PropertyName == "CurrentUser")
            {
                // Current user was destroyed, detach handlers to forget history and allow fresh assignment and return.
                if (UserManager.CurrentUser == null)
                {
                    UserManager.PropertyChanged -= UserManager_PropertyChanged;
                    return;
                }

                if (UserManager.CurrentUser.CoursesMetadata != null)
                {
                    if (_contentControlManager.CurrentControl != null)
                    {
                        _contentControlManager.RefreshCurrentControl();
                        SetTitleAndContent();
                    }
                    this._menu.GenerateView(null);
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
            return _contentControlManager.SaveState();
        }

        public async void LoadState(Dictionary<string, object> lastState)
        {
            if (_isCached == true || UserManager.IsBusy)
                return;

            StatusCode status = StatusCode.UnknownError;
            bool freshData = false;

            await Task.Run(async () =>
            {
                if (UserManager.CurrentUser.CoursesMetadata != null)
                {
                    if (lastState != null)
                        _contentControlManager.LoadState(lastState);
                    freshData = false;
                    status = StatusCode.Success;
                }
                else
                {
                    status = await UserManager.LoadCacheAsync();
                    if (status == StatusCode.Success)
                    {
                        freshData = false;
                        if (lastState != null)
                            _contentControlManager.LoadState(lastState);
                    }
                    else
                    {
                        status = await UserManager.RefreshFromServerAsync();
                        freshData = true;
                    }
                }
            });

            if (status == StatusCode.Success)
            {
                if (freshData == false && _contentControlManager.CanGoBack)
                    _contentControlManager.ReturnToLastControl();
                else
                    _contentControlManager.NavigateToControl(AppSettings.DefaultControlType, null);
                SetTitleAndContent();

                _menu.GenerateView(null);
                IsContentAvailable = true;
            }
            
            DisplayStatus(status, !freshData);
            UserManager.PropertyChanged += UserManager_PropertyChanged;
            _menu.ActionRequested += ProxiedControl_ActionRequested;

            loadingScreenPresenter.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _isCached = true;
            IsIdle = !UserManager.IsBusy;

            if (freshData == false && AppSettings.AutoRefresh == true)
                RefreshButton_Click(null, null);

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
            await _statusBar.ProgressIndicator.ShowAsync();

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
                    _statusBar.ProgressIndicator.Text = "Last refreshed " + DateTimeOffset.Now.ToString("HH:mm");
                else
                    _statusBar.ProgressIndicator.Text = "Last refreshed " + GetTimeString(UserManager.CachedDataLastChanged);
            }
            else
            {
                if (metaData == null)
                    _statusBar.ProgressIndicator.Text = "No data, unable to refresh";
                else
                    _statusBar.ProgressIndicator.Text = "Unable to refresh, last updated " + GetTimeString(UserManager.CachedDataLastChanged);
                StandardMessageDialogs.GetDialog(status).ShowAsync();
            }

            _statusBar.ProgressIndicator.ProgressValue = 0;
            _statusBar.ProgressIndicator.ShowAsync();
        }

        #endregion

        #region Navigation and ActionRequest Handlers

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(AboutPage), null, NavigationType.Default);
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(HelpPage), null, NavigationType.Default);
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

            if(e.TargetElement == typeof(HelpPage))
            {
                PageManager.NavigateTo(e.TargetElement, null, NavigationType.Default);
                return;
            }

            if (sender as MenuControl != null)
            {
                MenuButton_Click(null, null);
                _contentControlManager.ClearHistory();
            }

            _contentControlManager.NavigateToControl(e.TargetElement, e.Parameter);
            SetTitleAndContent();
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            if (_contentControlManager.CanGoBack)
            {
                _contentControlManager.ReturnToLastControl();
                SetTitleAndContent();
            }
        }

        private void ViewTodayButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMenuOpen == true)
                MenuButton_Click(null, null);

            if (ControlManager.GetCode(_contentControlManager.CurrentControl) == ControlTypeCodes.EnhancedTimetable)
                _contentControlManager.RefreshCurrentControl();
            else
            {
                object source = null;
                _contentControlManager.ClearHistory();
                ProxiedControl_ActionRequested(source, new RequestEventArgs(typeof(EnhancedTimetableControl), null));
            }
        }

        private void SetTitleAndContent()
        {
            string titleText = null;
            ControlTypeCodes contentTypeCode = ControlManager.GetCode(_contentControlManager.CurrentControl);

            switch (contentTypeCode)
            {
                case ControlTypeCodes.Overview:
                    titleText = "Overview";
                    break;
                case ControlTypeCodes.BasicTimetable:
                    titleText = "Timetable";
                    break;
                case ControlTypeCodes.EnhancedTimetable:
                    titleText = "Daily Buzz";
                    break;
                case ControlTypeCodes.CourseInfo:
                    titleText = "Course Details";
                    break;
                default:
                    titleText = "VITacademics";
                    break;
            }

            contentPresenter.Content = _contentControlManager.CurrentControl;
            NotifyPropertyChanged("CanGoBack");
            TitleText = titleText;
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
            else if (_contentControlManager.CurrentControl != null
                && ControlManager.GetCode(_contentControlManager.CurrentControl) != AppSettings.DefaultControlType)
            {
                _contentControlManager.ClearHistory();
                ProxiedControl_ActionRequested(this,
                           new RequestEventArgs(ControlManager.GetTypeFromCode(AppSettings.DefaultControlType), null));
                return false;
            }
            else
                return true;
        }

        #endregion

    }
}
