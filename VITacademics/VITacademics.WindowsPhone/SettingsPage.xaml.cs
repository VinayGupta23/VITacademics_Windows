using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using VITacademics.Helpers;
using VITacademics.Managers;
using VITacademics.UIControls;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Email;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;


namespace VITacademics
{
    
    public sealed partial class SettingsPage : Page, IManageable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        DataTransferManager _dataTransferManager;

        public SettingsPage()
        {
            this.InitializeComponent();

            StartupPageOptions = new Dictionary<string, string>();
            StartupPageOptions.Add("Overview", typeof(UserOverviewControl).FullName);
            StartupPageOptions.Add("Timetable", typeof(BasicTimetableControl).FullName);
            StartupPageOptions.Add("Daily Buzz", typeof(EnhancedTimetableControl).FullName);
            StartupPageOptions.Add("Grades", typeof(GradesControl).FullName);

            string defaultControlTypeName = AppSettings.DefaultControlTypeName;
            foreach (var pair in StartupPageOptions)
                if (pair.Value == defaultControlTypeName)
                {
                    CurrentDefaultView = pair.Key;
                    break;
                }

            AppSettings.SettingsChanged += AppSettings_SettingsChanged;
            this.Unloaded += SettingsPage_Unloaded;
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PageManager.RegisterPage(this);
            _dataTransferManager = DataTransferManager.GetForCurrentView();
            _dataTransferManager.DataRequested += DataTransferManager_DataRequested;

            if (e.Parameter != null && e.Parameter.ToString() == "feedback")
                rootPivot.SelectedIndex = 1;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            _dataTransferManager.DataRequested -= DataTransferManager_DataRequested;
            _dataTransferManager = null;
        }

        #region Settings Tab Members

        public Dictionary<string, string> StartupPageOptions
        {
            get;
            private set;
        }
        public string RegNo
        {
            get { return UserManager.CurrentUser.RegNo.ToUpper(); }
        }
        public bool AllowRefresh
        {
            get { return AppSettings.AutoRefresh; }
        }
        public string CurrentDefaultView
        {
            get;
            private set;
        }
        public bool IsNewSemesterAvailable
        {
            get { return AppSettings.IsSemesterUpgradeAvailable; }
        }

        void SettingsPage_Unloaded(object sender, RoutedEventArgs e)
        {
            AppSettings.SettingsChanged -= AppSettings_SettingsChanged;
        }

        void AppSettings_SettingsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSemesterUpgradeAvailable")
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("IsNewSemesterAvailable"));
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserManager.IsBusy == true)
            {
                await ShowBusyDialog();
                return;
            }

            MessageDialog msgDialog = new MessageDialog("This will log you out and delete all calendar appointments. Are you sure you want to continue?", "Logout?");
            msgDialog.Commands.Add(new UICommand("Logout", LogOutUser));
            msgDialog.Commands.Add(new UICommand("Cancel", LogOutUser));
            await msgDialog.ShowAsync();
        }

        private async void LogOutUser(IUICommand command)
        {
            if (command.Label == "Logout")
            {
                await StatusBar.GetForCurrentView().ProgressIndicator.HideAsync();
                await UserManager.DeleteSavedUserAsync();
                PageManager.NavigateTo(typeof(LoginPage), null, NavigationType.FreshStart);
            }
        }

        private void RefreshOption_Toggled(object sender, RoutedEventArgs e)
        {
            AppSettings.AutoRefresh = (sender as ToggleSwitch).IsOn;
        }

        private void PageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            AppSettings.DefaultControlTypeName = StartupPageOptions[(sender as ComboBox).SelectedItem as string];
        }

        private async void UpgradeButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserManager.IsBusy == true)
            {
                await ShowBusyDialog();
                return;
            }

            var status = await UserManager.RunMaintentanceForUpgradeAsync();
            if (status == Academics.ContentService.StatusCode.Success)
                PageManager.NavigateTo(typeof(MainPage), null, NavigationType.FreshStart);
            else
                await StandardMessageDialogs.GetDialog(Academics.ContentService.StatusCode.UnknownError).ShowAsync();
        }

        private async Task ShowBusyDialog()
        {
            await new MessageDialog("Please wait while your last requested task completes.", "Busy").ShowAsync();
        }

        #endregion

        #region Feedback Tab Members

        private const string DEV_EMAIL_ID = "vinaygupta_dev@outlook.com";
        private const string DEV_NAME = "Vinay Gupta";

        private async void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage emailMsg = new EmailMessage();

            emailMsg.Subject = "Feedback - VITacademics (Windows Phone)";
            emailMsg.To.Add(new EmailRecipient(DEV_EMAIL_ID, DEV_NAME));

            await EmailManager.ShowComposeNewEmailAsync(emailMsg);
        }

        private async void ReviewButton_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + MetadataProvider.AppId));
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void DataTransferManager_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            DataRequest request = args.Request;
            request.Data.Properties.Title = "VITacademics - Academic Metrics";
            request.Data.Properties.Description = "Try out the latest featured VITacademics app!";
            request.Data.SetWebLink(new Uri("https://vitacademics-rel.herokuapp.com/"));
        }

        #endregion

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {
        }

    }
}
