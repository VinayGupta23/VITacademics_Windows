using Academics.ContentService;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VITacademics.Common;
using VITacademics.Managers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace VITacademics
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page, IManageable
    {
        private string _regNo;

        public string Campus
        {
            get;
            set;
        }
        public string RegNo
        {
            get
            { return _regNo; }
            set
            {
                if (value != null)
                    _regNo = value.ToLower().Trim();
                UpdateLoginButtonState();
            }
        }
        public DateTimeOffset DOB
        {
            get;
            set;
        }

        public LoginPage()
        {
            this.InitializeComponent();

            datePicker.MaxYear = DateTimeOffset.UtcNow.AddYears(-1);
            SetState(true);

            this.DataContext = this;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PageManager.RegisterPage(this);
        }

        private bool IsRegNoValid()
        {
            bool valid = false;
            if (RegNo.Length > 5)
            {
                int i;
                for (i = 0; i < RegNo.Length; i++)
                {
                    if (i >= 2 && i <= 4)
                    {
                        if (!char.IsLetter(RegNo[i]))
                            break;
                    }
                    else
                    {
                        if (!char.IsNumber(RegNo[i]))
                            break;
                    }
                }
                if (i == RegNo.Length)
                    valid = true;
            }
            return valid;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Campus = ((sender as RadioButton).Content as string).ToLower();
            UpdateLoginButtonState();
        }

        private void UpdateLoginButtonState()
        {
            loginButton.IsEnabled =
                Campus != null && RegNo != null;
        }

        private void SetState(bool isIdle)
        {
            regNoBox.IsEnabled = isIdle;
            datePicker.IsEnabled = isIdle;
            radioButton1.IsEnabled = isIdle;
            radioButton2.IsEnabled = isIdle;

            if (isIdle)
            {
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                statusBlock.Text = "Login to get started";
                UpdateLoginButtonState();
            }
            else
            {
                progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                statusBlock.Text = "Logging in";
                loginButton.IsEnabled = false;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            // To prevent on-screen keyboard from automatically displaying after the method executes.
            regNoBox.IsTabStop = false;
            SetState(false);

            if (IsRegNoValid() == false)
            {
                new MessageDialog("Please enter your register number in the correct format and try again.", "Invalid Credentials").ShowAsync();
            }
            else
            {
                StatusCode statusCode = await Task.Run(() => UserManager.CreateNewUserAsync(RegNo, DOB, Campus));

                if (statusCode == StatusCode.Success)
                    PageManager.NavigateTo(typeof(MainPage), null, NavigationType.FreshStart);
                else
                    StandardMessageDialogs.GetDialog(statusCode).ShowAsync();
            }

            SetState(true);
            // Re-enable TextBox.
            regNoBox.IsTabStop = true;

        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }
        public void LoadState(Dictionary<string, object> lastState)
        { }

    }


}
