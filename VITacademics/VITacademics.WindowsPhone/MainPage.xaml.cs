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
using VITacademics.Common;
using VITacademics.Helpers;


namespace VITacademics
{

    public sealed partial class MainPage : Page, IManageable
    {
        private StatusBar _statusBar = StatusBar.GetForCurrentView();
        public static PivotContentManager PivotManager
        {
            get;
            private set;
        }


        public MainPage()
        {
            this.InitializeComponent();

            MainPage.PivotManager = new PivotContentManager(mainPivot);

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
            PageManager.NavigateTo(typeof(LoginPage), null, NavigationType.FreshStart);   
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }
        public void LoadState(Dictionary<string, object> lastState)
        {
            
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton source = (sender as AppBarButton);

            _statusBar.ProgressIndicator.Text = "Refreshing...";
            _statusBar.ProgressIndicator.ProgressValue = null;
            _statusBar.ProgressIndicator.ShowAsync();
            source.IsEnabled = false;

            StatusCode code = await Task.Run(() => UserManager.RefreshFromServerAsync());

            if (code != StatusCode.Success)
            {
                if (UserManager.CurrentUser.CoursesMetadata == null)
                    _statusBar.ProgressIndicator.Text = "Unable to refresh, last tried at " + DateTimeOffset.Now.ToString("H:mm");
                else
                    _statusBar.ProgressIndicator.Text = "Unable to refresh, last updated " + GetDisplayString(UserManager.CurrentUser.CoursesMetadata.RefreshedDate);
                StandardMessageDialogs.GetDialog(code).ShowAsync();
            }
            else
            {
                _statusBar.ProgressIndicator.Text = "Last refreshed " + GetDisplayString(UserManager.CurrentUser.CoursesMetadata.RefreshedDate);
            }

            _statusBar.ProgressIndicator.ProgressValue = 0;
            source.IsEnabled = true;
        }

        private string GetDisplayString(DateTimeOffset date)
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
    }
}
