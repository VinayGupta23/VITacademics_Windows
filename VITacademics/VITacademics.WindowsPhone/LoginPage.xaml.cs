using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace VITacademics
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        private bool _isRegNoValid;
        private string _regNo;

        // Use a check box (Like "Remember Me"). If this is a temporary session, store all data to the TempFolder only.
        public bool ShouldSaveCredentials
        {
            get;
            set;
        }
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
                _regNo = value.Trim();
                ValidateRegNo();
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
            this.DataContext = this;

            UpdateLoginButtonState();
            // Set upper limit for DateTime picker.
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void ValidateRegNo()
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
            _isRegNoValid = valid;
            UpdateLoginButtonState();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            Campus = (sender as RadioButton).Content as string;
            UpdateLoginButtonState();
        }

        private void UpdateLoginButtonState()
        {
            bool state =
                (Campus != null) && _isRegNoValid;
            // loginButton.IsEnabled = state;
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
