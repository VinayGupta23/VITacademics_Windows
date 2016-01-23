using Academics.ContentService;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;

namespace VITacademics.Helpers
{
    public static class StandardMessageDialogs
    {
        public static MessageDialog GetDialog(StatusCode code)
        {
            switch (code)
            {
                case StatusCode.InvalidCredentials:
                    return new MessageDialog("Please check your credentials and try again.", "Invalid Credentials");
                case StatusCode.ServerError:
                case StatusCode.UnderMaintenance:
                case StatusCode.SessionTimeout:
                    return new MessageDialog("The servers are overloaded or currently under maintenance. Please try again after some time.", "Sorry");
                case StatusCode.NoInternet:
                    return new MessageDialog("We couldn't connect to the servers. Please check your internet connection and try again.", "No Internet");
                default:
                    return new MessageDialog("Oops. Something unexpected happened. If you're seeing this too often, do contact us.", "Unknown Error");
            }
        }
    }
}
