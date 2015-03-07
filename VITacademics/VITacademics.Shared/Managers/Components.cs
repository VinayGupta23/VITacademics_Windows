using Academics.ContentService;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;


namespace VITacademics.Managers
{
    /// <summary>
    /// A contract required to be implemented by every page to take part in the page management process.
    /// </summary>
    /// <remarks>
    ///  Note: The behaviour of these methods are not affected whether the page requested for caching or not.
    /// </remarks>
    public interface IManageable
    {
        /// <summary>
        /// This method should return any page specific state that is required to be stored. For correct behaviour, it is required that the state dictionary only contain primitive types.
        /// </summary>
        /// <remarks>
        /// This method is called by the PageManager on navigating away to another page (Not on going back).
        /// </remarks>
        /// <returns></returns>
        Dictionary<string, object> SaveState();
        /// <summary>
        /// This method is called by the PageManager when this page becomes the current page.
        /// </summary>
        /// <param name="lastState">
        /// The page state when it was last navigated to (as provided by the page).
        /// </param>
        /// <remarks>
        /// This method is always invoked when the page is loaded, and hence null checking is advisable to ensure a last state actually exists.
        /// </remarks>
        void LoadState(Dictionary<string, object> lastState);
    }

    /// <summary>
    /// Types of navigation available when navigating to a page.
    /// </summary>
    public enum NavigationType
    {
        /// <summary>
        /// Simple navigates to the requested page and stores the last page's session.
        /// </summary>
        Default,
        /// <summary>
        /// Navigates to the requested page and resets all history and saved state. Use this mode even for a first time navigation.
        /// </summary>
        FreshStart
    }

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
                    return new MessageDialog("The servers are overloaded or currently under maintenance. Please try again after some time.", "Sorry");
                case StatusCode.NoInternet:
                    return new MessageDialog("We couldn't connect to our servers. Please check your internet connection and try again.", "No Internet");
                default:
                    return new MessageDialog("An unforeseen error has occured, please try again later. If this problem persists, feel free to contact us so we can provide you some assistance.", "Unknown Error");
            }
        }
    }

#if WINDOWS_PHONE_APP
    /// <summary>
    /// Provides the contract for a page to allow or prevent app exit by pressing the back button.
    /// </summary>
    /// <remarks>
    /// Note: This method must not require to be awaited, otherwise unpredictable behaviour may occur. 
    /// </remarks>
    public interface IAppReturnControllable
    {
        /// <summary>
        /// This method is called when the user presses the back button to navigate out of the app. Return false to cancel this behaviour.
        /// </summary>
        bool AllowAppExit();
    }

    public interface IProxiedControl
    {
        event EventHandler<RequestEventArgs> ActionRequested;
    }

    public class RequestEventArgs : EventArgs
    {
    }

#endif

}
