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


namespace VITacademics
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IManageable
    {

        private StatusBar _statusBar = StatusBar.GetForCurrentView();

        public MainPage()
        {
            this.InitializeComponent();

            _statusBar.BackgroundColor = (Application.Current.Resources["AlternateDarkBrush"] as SolidColorBrush).Color;
            _statusBar.ForegroundColor = Colors.LightGray;
            _statusBar.ShowAsync();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.
            PageManager.RegisterPage(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {    
            UserManager.DeleteSavedUser();
            _statusBar.ProgressIndicator.HideAsync();
            PageManager.NavigateTo(typeof(LoginPage), null, NavigationType.FreshStart);   
        }

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
                mainContentPresenter.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton sourceButton = (sender as AppBarButton);

            sourceButton.IsEnabled = false;
            _statusBar.ProgressIndicator.Text = "Refreshing...";
            _statusBar.ProgressIndicator.ProgressValue = null;
            _statusBar.ProgressIndicator.ShowAsync();

            StatusCode code = await Task.Run(() => UserManager.RefreshFromServerAsync());

            DisplayStatus(code, false);
            sourceButton.IsEnabled = true;
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
                    _statusBar.ProgressIndicator.Text = "Loaded cache, last refreshed " + GetTimeString(metaData.RefreshedDate);
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

    }
}
