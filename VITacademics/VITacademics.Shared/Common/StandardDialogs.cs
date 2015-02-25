using Academics.ContentService;
using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Popups;

namespace VITacademics.Common
{
    static class StandardMessageDialogs
    {
        public static MessageDialog GetDialog(StatusCode code)
        {
            switch (code)
            {
                case StatusCode.InvalidCredentials:
                    return new MessageDialog("Please check your credentials and try again.", "Invalid Credentials");
                case StatusCode.ServerError:
                case StatusCode.UnderMaintenance:
                    return new MessageDialog("The servers are overloaded or currently under maintenance. Please try again after some time.", "Oops...");
                case StatusCode.NoInternet:
                    return new MessageDialog("We can't connect to our servers. Please check your internet connection and try again.", "No Internet");
                default:
                    return new MessageDialog("An unforeseen error has occured, please try again later. If this problem persists, feel free to contact us so we can provide you some assistance.", "Unknown Error");
            }
        }
    }
}
