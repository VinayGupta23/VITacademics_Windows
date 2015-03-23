using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VITacademics.Managers;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;


namespace VITacademics
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public sealed partial class App : Application
    {

        public static readonly ApplicationDataContainer _localSettings = ApplicationData.Current.LocalSettings;
        public static readonly ApplicationDataContainer _roamingSettings = ApplicationData.Current.RoamingSettings;

#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used when the application is launched to open a specific file, to display
        /// search results, and so forth.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {

#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif

            Frame rootFrame = Window.Current.Content as Frame;

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context via PageManager.
                rootFrame = PageManager.Initialize();

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    if (CheckSessionValidity() == true &&
                        // Do not restore session state if the last run session was long back.
                        (DateTimeOffset.UtcNow - PageManager.LastSessionSavedDate).Hours <= 24)
                    {
                        bool restoreResult = await PageManager.TryRestoreState();
                        if (restoreResult == false)
                            rootFrame = PageManager.Initialize();
                    }
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Removes the turnstile navigation for startup.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // Load the desired page by checking some conditions.
                if (UserManager.CurrentUser != null)
                    PageManager.NavigateTo(typeof(MainPage), null, NavigationType.FreshStart);
                else
                    PageManager.NavigateTo(typeof(LoginPage), null, NavigationType.FreshStart);

            }

            // Ensure the current window is active
            Window.Current.Activate();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Restores the content transitions after the app has launched.
        /// </summary>
        /// <param name="sender">The object where the handler is attached.</param>
        /// <param name="e">Details about the navigation event.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }
#endif

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private async void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            await PageManager.SaveSessionState();

            deferral.Complete();
        }

        /// <summary>
        /// Checks if the last saved session state is valid in terms of data integrity.
        /// </summary>
        private bool CheckSessionValidity()
        {
            bool isSessionValid = false;
            if (UserManager.CurrentUser != null)
            {
                if (string.Equals(UserManager.CurrentUser.RegNo, PageManager.LastSessionOwner, StringComparison.OrdinalIgnoreCase) == true)
                    // To ensure the background service did not update the data cache,
                    // Otherwise the last session (relying on the old cache) becomes invalid.
                    if (DateTimeOffset.Equals(PageManager.SessionRelevancyDate, UserManager.CachedDataLastChanged) == true)
                    {
                        isSessionValid = true;
                    }
            }
            return isSessionValid;
        }

    }
}