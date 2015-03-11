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


namespace VITacademics
{

    public sealed partial class MainPage : Page, IManageable, INotifyPropertyChanged
    {

        private StatusBar _statusBar = StatusBar.GetForCurrentView();

        public bool IsIdle
        {
            get { return !UserManager.IsBusy; }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;

            UserManager.PropertyChanged += UserManager_PropertyChanged;

            _statusBar.BackgroundColor = (Application.Current.Resources["AlternateDarkBrush"] as SolidColorBrush).Color;
            _statusBar.ForegroundColor = Colors.LightGray;
            _statusBar.ShowAsync();
        }


        void UserManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBusy")
                NotifyPropertyChanged("IsIdle");
            if(e.PropertyName == "CurrentUser")
            {

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

        private void NotifyPropertyChanged(string propertyName)
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
            DateTimeOffset today = DateTimeOffset.UtcNow;
            if (date.Month == today.Month)
            {
                if (date.Day == today.Day)

                    return "at " + date.ToLocalTime().ToString("H:mm");
                else
                    return "on " + date.ToLocalTime().ToString("ddd, H:mm");
            }
            else
                return "on " + date.ToLocalTime().ToString("dd MMM, H:mm");
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
                    _statusBar.ProgressIndicator.Text = "Unable to refresh, last tried at " + DateTimeOffset.Now.ToString("H:mm");
                else
                    _statusBar.ProgressIndicator.Text = "Unable to refresh, last updated " + GetTimeString(metaData.RefreshedDate);
                StandardMessageDialogs.GetDialog(status).ShowAsync();
            }

            _statusBar.ProgressIndicator.ProgressValue = 0;
            _statusBar.ProgressIndicator.ShowAsync();
        }

        #endregion

        #region Navigation Request Handlers

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(AboutPage), null, NavigationType.Default);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(SettingsPage), null, NavigationType.Default);
        }

        // Temporary for testing only.
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            if (UserManager.IsBusy == true)
            {
                StandardMessageDialogs.GetDialog(StatusCode.UnknownError).ShowAsync();
                return;
            }

            UserManager.DeleteSavedUser();
            _statusBar.ProgressIndicator.HideAsync();
            PageManager.NavigateTo(typeof(LoginPage), null, NavigationType.FreshStart);

        }
             


        #endregion

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {

            BasicTimetableControl b = new BasicTimetableControl();
            contentPresenter.Content = b;
            b.GenerateTimetableView(Timetable.GetTimetable(UserManager.CurrentUser.Courses));

        }

    }
}
