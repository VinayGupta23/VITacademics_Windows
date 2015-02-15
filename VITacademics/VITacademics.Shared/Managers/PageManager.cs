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
    ///     <code>
    ///     protected override void OnNavigatedTo(NavigationEventArgs e)
    ///     {
    ///         PageManager.RegisterPage(this);
    ///
    ///         // Perform other actions here.
    ///     }
    ///     </code>
    ///</remarks>
    public static class PageManager
    {
        private const string NAV_FILE_NAME = "NavHistory.xml";
        private const string STATE_FILE_NAME = "SessionState.xml";
        private static readonly StorageFolder Folder = ApplicationData.Current.LocalFolder;

        private static Page _currentPage;
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

        static PageManager()
        {

#if WINDOWS_PHONE_APP
            Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif
        }

#if WINDOWS_PHONE_APP
        private static void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            try
            {
                if(CanNavigateBack)
                {
                    NavigateBack();
                    e.Handled = true;
                }
                else
                {
                    bool allowExit = (_currentPage as IManageable).AllowAppExit();
                    e.Handled = !allowExit;
                }
            }
            catch
            { e.Handled = false; }
        }
#endif

        private static void CurrentPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_pageState != null)
                (sender as IManageable).LoadState(_pageState);
            _currentPage.Loaded -= CurrentPage_Loaded;
        }

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
        /// This method calls SaveState() on the current page to cache and page specific state.
        /// </remarks>
        public static void NavigateTo(Type pageType, object parameter, NavigationType type)
        {

            if (type == NavigationType.Default)
            {
                Dictionary<string, object> pageState = (_currentPage as IManageable).SaveState();
                RootFrame.Navigate(pageType, parameter);
                PageStates.Add(pageState);
            }
            else
            {
                RootFrame.Navigate(pageType, parameter);
                RootFrame.BackStack.Clear();
                PageStates.Clear();
            }
        }

        /// <summary>
        /// Navigates to the last visited page in the back stack. This method does nothing if there is no page to go back to.
        /// </summary>
        public static void NavigateBack()
        {
            try
            {
                int lastPageIndex = RootFrame.BackStackDepth - 1;
                _pageState = PageStates[lastPageIndex];

                RootFrame.GoBack();
                PageStates.RemoveAt(lastPageIndex);
            }
            catch { }
        }

        /// <summary>
        /// Call this method to save the navigation history along with session state locally.
        /// </summary>
        /// <returns></returns>
        public static async Task SaveSessionState()
        {
            try
            {
                PageStates.Add((_currentPage as IManageable).SaveState());
                StorageFile navFile = await Folder.CreateFileAsync(NAV_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                StorageFile stateFile = await Folder.CreateFileAsync(STATE_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                await StorageHelper.TryWriteAsync(navFile, RootFrame.GetNavigationState());
                await StorageHelper.TryWriteAsync(stateFile, PageStates);
            }
            catch
            { }
        }

        /// <summary>
        /// Use this method to restore session state and navigation histoy. This method should be called after Initialize()./>
        /// </summary>
        /// <returns>
        /// True if app state was successfully restored.
        /// </returns>
        public static async Task<bool> TryRestoreState()
        {
            try
            {
                StorageFile navFile = await Folder.GetFileAsync(NAV_FILE_NAME);
                StorageFile stateFile = await Folder.GetFileAsync(STATE_FILE_NAME);

                PageStates = await StorageHelper.TryReadAsync<List<Dictionary<string, object>>>(stateFile);
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

    }
}
