using System;
using System.Collections.Generic;
using VITacademics.Managers;
using Windows.ApplicationModel.Email;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace VITacademics
{

    public sealed partial class AboutPage : Page, IManageable
    {

        private const string DEV_EMAIL_ID = "vinaygupta_dev@outlook.com";
        private const string DEV_NAME = "Vinay Gupta";
        private StatusBar _statusBar;

        public AboutPage()
        {
            this.InitializeComponent();
            _statusBar = StatusBar.GetForCurrentView();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            PageManager.RegisterPage(this);
            await _statusBar.ProgressIndicator.HideAsync();
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            await _statusBar.ProgressIndicator.ShowAsync();
            _statusBar = null;
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {
        }

        private async void EmailButton_Click(object sender, RoutedEventArgs e)
        {
            EmailMessage emailMsg = new EmailMessage();
            
            emailMsg.Subject = "Feedback - VITacademics (Windows Phone)";
            emailMsg.To.Add(new EmailRecipient(DEV_EMAIL_ID, DEV_NAME));

            await EmailManager.ShowComposeNewEmailAsync(emailMsg);
        }
    }
}
