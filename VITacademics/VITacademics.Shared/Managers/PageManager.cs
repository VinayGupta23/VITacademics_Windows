using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Windows.Storage;
using System.Threading.Tasks;


namespace VITacademics.Managers
{

    /// <summary>
    /// Provides mechanisms to track and store session state for every registered page. This class also manages app lifecycle.
    /// </summary>
    /// <remarks>
    /// To use the page management service, make the following changes to every page in the app:
    /// 
    ///     1. Implement interface <see cref="IManageable"/>.
    ///     2. Override the <see cref="OnNavigatedTo"/> method in the page as follows:
    ///        <code>
    ///        protected override void OnNavigatedTo(NavigationEventArgs e)
    ///        {
    ///            PageManager.RegisterPage(this);
    ///
    ///            // Perform other actions here.
    ///        }
    ///        </code>
    ///     3. Any navigation between pages must take place through the static wrapper methods,
    ///        namely <see cref="NavigateTo"/> and <see cref="NavigateBack"/>
    ///        
    /// Note: For Windows Phone, pages can optionally implement the <see cref="IAppReturnControllable"/> to prevent or allow the app from suspending when the user presses the back button.
    ///</remarks>
    public static class PageManager
    {
        #region Constants

        private const string NAV_FILE_NAME = "NavHistory.xml";
        private const string STATE_FILE_NAME = "SessionState.xml";
        private const string SESSION_LASTDATE_KEY = "sessionState_lastDate";
        private const string SESSION_OWNER_KEY = "sessionState_owner";
        private const string SESSION_RELEVANCY_DATE_KEY = "sessionState_relevancyDate";

        #endregion

        #region Fields and Properties

        private static readonly StorageFolder _folder = ApplicationData.Current.LocalFolder;
        private static readonly Type[] _standardKnownTypes;

        private static Page _currentPage;
        private static NavigationType _currentType;
        private static Dictionary<string, object> _pageState;

        private static Frame RootFrame
        {
            get;
            set;
        }
        private static List<Dictionary<string, Object>> PageStates
        {
            get;
            set;
        }

        public static bool CanNavigateBack
        {
            get
            {
                return RootFrame.CanGoBack;
            }
        }
        /// <summary>
        /// Gets the last point of time the session state was saved, in UTC format. If unavailable, gets the default value of the data type.
        /// </summary>
        public static DateTimeOffset LastSessionSavedDate
        {
            get
            {
                try
                {
                    return (DateTimeOffset)App._localSettings.Values[SESSION_LASTDATE_KEY];
                }
                catch
                {
                    return default(DateTimeOffset);
                }
            }
            private set
            {
                App._localSettings.Values[SESSION_LASTDATE_KEY] = value;
            }
        }
        /// <summary>
        /// Gets the last user's register number, who's session state was saved.
        /// </summary>
        public static string LastSessionOwner
        {
            get
            {
                try
                {
                    return App._localSettings.Values[SESSION_OWNER_KEY] as string;
                }
                catch
                {
                    return null;
                }
            }
            private set
            {
                App._localSettings.Values[SESSION_OWNER_KEY] = value;
            }
        }
        /// <summary>
        /// Gets the age of the data for which the session remains valid.
        /// </summary>
        public static DateTimeOffset SessionRelevancyDate
        {
            get
            {
                try
                {
                    return (DateTimeOffset)App._localSettings.Values[SESSION_RELEVANCY_DATE_KEY];
                }
                catch
                {
                    return default(DateTimeOffset);
                }
            }
            private set
            {
                App._localSettings.Values[SESSION_RELEVANCY_DATE_KEY] = value;
            }
        }

        #endregion

        #region Contructor and Dependencies

        static PageManager()
        {

#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
            _standardKnownTypes = new Type[4];
            _standardKnownTypes[0] = typeof(List<bool>);
            _standardKnownTypes[1] = typeof(List<int>);
            _standardKnownTypes[2] = typeof(List<string>);
            _standardKnownTypes[3] = typeof(List<double>);
        }

#if WINDOWS_PHONE_APP
        private static void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            if(CanNavigateBack)
            {
                NavigateBack();
                e.Handled = true;
            }
            else
            {
                if(_currentPage as IAppReturnControllable != null)
                {
                    bool allowExit = (_currentPage as IAppReturnControllable).AllowAppExit();
                    e.Handled = !allowExit;
                }
            }
        }
#endif

        #endregion

        #region Private Helper Methods

        private static void CurrentPage_Loaded(object sender, RoutedEventArgs e)
        {
            (sender as IManageable).LoadState(_pageState);
            _currentPage.Loaded -= CurrentPage_Loaded;
        }

        private static void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (_currentType == NavigationType.FreshStart)
            {
                RootFrame.BackStack.Clear();
                PageStates.Clear();
            }
        }

        private static void ClearPageCache()
        {
            RootFrame.CacheSize = 0;
            RootFrame.CacheSize = 1;
        }

        #endregion

        #region Public Methods (API)

        /// <summary>
        /// Call this method to register the current page to allow management of state.
        /// </summary>
        /// <param name="page">
        /// The reference to the current page.
        /// </param>
        public static void RegisterPage(Page page)
        {
            _currentPage = page;
            _currentPage.Loaded += CurrentPage_Loaded;
        }

        public static Frame Initialize()
        {
            RootFrame = new Frame();
            RootFrame.CacheSize = 1;
            RootFrame.Navigated += RootFrame_Navigated;
            PageStates = new List<Dictionary<string, object>>();
            return RootFrame;
        }
        
        /// <summary>
        /// Navigates to the desired page, passing a parameter to the next page.
        /// </summary>
        /// <param name="pageType">
        /// The type of page to navigate to.
        /// </param>
        /// <param name="parameter">
        /// The parameter to pass to the page being navigated to.
        /// </param>
        /// <param name="type">
        /// The type of navigation to use.
        /// </param>
        /// <remarks>
        /// This method calls SaveState() on the current page to store page specific state.
        /// </remarks>
        public static void NavigateTo(Type pageType, object parameter, NavigationType type)
        {
            _pageState = null;
            _currentType = type;

            if (type == NavigationType.Default)
            {
                Dictionary<string, object> pageState = (_currentPage as IManageable).SaveState();
                RootFrame.Navigate(pageType, parameter);
                PageStates.Add(pageState);
            }
            else
            {
                ClearPageCache();
                RootFrame.Navigate(pageType, parameter);
                // Clearing of back stack and page states occur in Navigated event handler.
            }
        }

        /// <summary>
        /// Navigates to the last visited page in the back stack. This method does nothing if there is no page to go back to.
        /// </summary>
        public static void NavigateBack()
        {
            if (RootFrame.CanGoBack)
            {
                int lastPageIndex = RootFrame.BackStackDepth - 1;
                _pageState = PageStates[lastPageIndex];

                RootFrame.GoBack();
                PageStates.RemoveAt(lastPageIndex);
            }
        }

        /// <summary>
        /// Call this method to save the navigation history along with session state locally. On failure, the Last Saved date is set to default (of the data type).
        /// </summary>
        /// <returns></returns>
        public static async Task SaveSessionState()
        {
            try
            {
                LastSessionSavedDate = default(DateTimeOffset);
                SessionRelevancyDate = default(DateTimeOffset);
                LastSessionOwner = null;

                PageStates.Add((_currentPage as IManageable).SaveState());
                StorageFile navFile = await _folder.CreateFileAsync(NAV_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                StorageFile stateFile = await _folder.CreateFileAsync(STATE_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                bool result = true;
                result &= await StorageHelper.TryWriteAsync(navFile, RootFrame.GetNavigationState());
                result &= await StorageHelper.TryWriteAsync(stateFile, PageStates, _standardKnownTypes);
                
                if (result == true)
                {
                    LastSessionSavedDate = DateTimeOffset.UtcNow;
                    SessionRelevancyDate = UserManager.CachedDataLastChanged;
                    LastSessionOwner = UserManager.CurrentUser.RegNo;
                }
            }
            catch { }
        }

        /// <summary>
        /// Use this method to restore session state and navigation history. This method should only be called if Initialize() has already been executed./>
        /// </summary>
        /// <returns>
        /// True if app state was successfully restored.
        /// </returns>
        public static async Task<bool> TryRestoreState()
        {
            try
            {
                StorageFile navFile = await _folder.GetFileAsync(NAV_FILE_NAME);
                StorageFile stateFile = await _folder.GetFileAsync(STATE_FILE_NAME);

                PageStates = await StorageHelper.TryReadAsync<List<Dictionary<string, object>>>(stateFile, _standardKnownTypes);
                int topIndex = PageStates.Count - 1;
                _pageState = PageStates[topIndex];
                PageStates.RemoveAt(topIndex);

                string navHistory = await StorageHelper.TryReadAsync(navFile);
                RootFrame.SetNavigationState(navHistory);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Manages the page history suitably when the App resumes from a suspended state. Ensure to call this method so that page states are recorded accurately.
        /// </summary>
        /// <remarks>
        /// Ideally, the method call must be placed in the Resuming event handler for the App.
        /// </remarks>
        public static void ResumeState()
        {
            // The current page's state is added to the history when suspending.
            // On resumption, the state is not required as the code continues running from its last point.
            if (PageStates.Count > 0)
                PageStates.RemoveAt(PageStates.Count - 1);
        }

        #endregion
    }
}
