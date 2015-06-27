using Academics.ContentService;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using VITacademics.Helpers;
using VITacademics.Managers;
using VITacademics.UIControls;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using System.Text;
using System.Linq;

namespace VITacademics
{

    public sealed partial class MainPage : Page, IManageable, INotifyPropertyChanged, IAppReturnControllable
    {

        private const string TITLE_SEPARATOR = "  >  ";

        private StatusBar _statusBar;
        private MenuControl _menu;
        private ControlManager _contentControlManager;
        private bool _isMenuOpen;
        private bool _isIdle;
        private bool _isContentAvailable;
        private TitleBuilder _titleBuilder = new TitleBuilder(TITLE_SEPARATOR);
        private bool _isCached = false;

        public bool IsIdle
        {
            get { return _isIdle; }
            private set
            {
                _isIdle = value;
                NotifyPropertyChanged();
            }
        }
        private bool IsMenuOpen
        {
            get { return _isMenuOpen; }
            set
            {
                _isMenuOpen = value;
                NotifyPropertyChanged("TitleText");
            }
        }
        public string TitleText
        {
            get
            {
                if (IsMenuOpen == true) return "VITACADEMICS";
                else return _titleBuilder.Title;
            }
        }
        public bool IsContentAvailable
        {
            get { return _isContentAvailable; }
            set
            {
                _isContentAvailable = value;
                NotifyPropertyChanged();
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.NavigationCacheMode = Windows.UI.Xaml.Navigation.NavigationCacheMode.Enabled;

            _menu = new MenuControl();
            _contentControlManager = new ControlManager(ProxiedControl_ActionRequested);

            _statusBar = StatusBar.GetForCurrentView();
            _statusBar.BackgroundColor = (Application.Current.Resources["AlternateDarkBrush"] as SolidColorBrush).Color;
            _statusBar.ForegroundColor = Colors.LightGray;
            _statusBar.ShowAsync();
        }

        void UserManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsBusy")
                this.IsIdle = !UserManager.IsBusy;
            else if (e.PropertyName == "IsContentReady")
                this.IsContentAvailable = UserManager.IsContentReady;
            else if (e.PropertyName == "CurrentUser")
            {
                // Current user was destroyed, detach handlers to forget history and allow fresh assignment and return.
                if (UserManager.CurrentUser == null)
                {
                    UserManager.PropertyChanged -= UserManager_PropertyChanged;
                    return;
                }

                if (UserManager.CurrentUser.CoursesMetadata != null)
                    if (_contentControlManager.CurrentControl != null)
                        _contentControlManager.RefreshCurrentControl();
                    else
                    {
                        _contentControlManager.Clear();
                        _titleBuilder.Clear();
                        _contentControlManager.NavigateToControl(AppSettings.DefaultControlTypeName, null);
                        _titleBuilder.SetTitle(_contentControlManager.CurrentControl.DisplayTitle);
                        SetTitleAndContent();
                    }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PageManager.RegisterPage(this);
        }

        #region IManageable Interface Implementation

        public Dictionary<string, object> SaveState()
        {
            var dictionary = _contentControlManager.SaveState();
            if (dictionary == null)
                dictionary = new Dictionary<string, object>();
            dictionary.Add("pageTitleComponents", _titleBuilder.Components);
            return dictionary;
        }

        public async void LoadState(Dictionary<string, object> lastState)
        {
            if (_isCached == true || UserManager.IsBusy)
                return;

            StatusCode status = StatusCode.UnknownError;
            bool freshData = false;

            await Task.Run(async () =>
            {
                if (UserManager.CurrentUser.CoursesMetadata != null)
                {
                    freshData = false;
                    status = StatusCode.Success;
                }
                else
                {
                    status = await UserManager.LoadCacheAsync();
                    if (status == StatusCode.Success)
                    {
                        freshData = false;
                    }
                    else
                    {
                        status = await UserManager.RefreshFromServerAsync();
                        freshData = true;
                    }
                }
            });

            if (status == StatusCode.Success)
            {
                if (freshData == false)
                {
                    if (lastState != null)
                    {
                        _contentControlManager.LoadState(lastState);
                        _titleBuilder = new TitleBuilder(TITLE_SEPARATOR, lastState["pageTitleComponents"] as IEnumerable<string>);
                    }
                    if (_contentControlManager.CanGoBack)
                        _contentControlManager.ReturnToLastControl();
                    else
                    {
                        _contentControlManager.NavigateToControl(AppSettings.DefaultControlTypeName, null);
                        _titleBuilder.SetTitle(_contentControlManager.CurrentControl.DisplayTitle);
                    }
                }
                else
                {
                    _contentControlManager.NavigateToControl(AppSettings.DefaultControlTypeName, null);
                    _titleBuilder.SetTitle(_contentControlManager.CurrentControl.DisplayTitle);
                }

                SetTitleAndContent();
                _menu.LoadView(null);
                IsContentAvailable = true;
            }

            await DisplayStatusAsync(status, !freshData);
            UserManager.PropertyChanged += UserManager_PropertyChanged;
            _menu.ActionRequested += ProxiedControl_ActionRequested;

            loadingScreenPresenter.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            _isCached = true;
            IsIdle = !UserManager.IsBusy;

            if (freshData == false && AppSettings.AutoRefresh == true && AppSettings.IsSemesterUpgradeAvailable == false)
                RefreshButton_Click(null, null);
        }

        #endregion

        #region INotifyPropertyChanged Interface Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Refresh Event Handler and Dependencies

        private async Task PromptNewSemesterAvailabilityAsync()
        {
            if (Frame.CurrentSourcePageType == typeof(SettingsPage))
                return;

            MessageDialog semUpgradeDialog = new MessageDialog("The current semester has closed and hence its support has ended. New data is now available.\n\nVisit 'settings' to upgrade the app when you're ready. You can continue viewing the current details until then.", "Semester Upgrade");
            semUpgradeDialog.Commands.Add(new UICommand("settings", UpgradeCommandHandler));
            semUpgradeDialog.Commands.Add(new UICommand("upgrade later", UpgradeCommandHandler));
            await semUpgradeDialog.ShowAsync();
        }

        private void UpgradeCommandHandler(IUICommand command)
        {
            if (command.Label == "settings")
                PageManager.NavigateTo(typeof(SettingsPage), null, NavigationType.Default);
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (AppSettings.IsSemesterUpgradeAvailable)
            {
                PromptNewSemesterAvailabilityAsync();
                return;
            }
            _statusBar.ProgressIndicator.Text = "Refreshing...";
            _statusBar.ProgressIndicator.ProgressValue = null;
            await _statusBar.ProgressIndicator.ShowAsync();

            StatusCode code = await UserManager.RefreshFromServerAsync();

            await DisplayStatusAsync(code, false);
        }

        private string GetTimeString(DateTimeOffset date)
        {
            date = date.ToLocalTime();
            DateTimeOffset today = DateTimeOffset.Now;

            if (today.Date == date.Date)
                return "at " + date.ToString("HH:mm");
            else if (today.Date.Subtract(new TimeSpan(6, 0, 0, 0)) <= date)
                return "on " + date.ToString("ddd, HH:mm");
            else
                return "on " + date.ToString("dd MMM, HH:mm");
        }
        private async Task DisplayStatusAsync(StatusCode status, bool refreshedFromCache)
        {
            if (status == StatusCode.Success)
            {
                _statusBar.ProgressIndicator.Text = "Last refreshed " + GetTimeString(UserManager.CachedDataLastChanged);
                if (AppSettings.IsSemesterUpgradeAvailable)
                    PromptNewSemesterAvailabilityAsync();
            }
            else
            {
                if (UserManager.CurrentUser.CoursesMetadata == null)
                    _statusBar.ProgressIndicator.Text = "No data, unable to refresh";
                else
                    _statusBar.ProgressIndicator.Text = "Refresh failed, last updated " + GetTimeString(UserManager.CachedDataLastChanged);

                StandardMessageDialogs.GetDialog(status).ShowAsync();
            }

            _statusBar.ProgressIndicator.ProgressValue = 0;
            await _statusBar.ProgressIndicator.ShowAsync();
        }

        #endregion

        #region Navigation and ActionRequest Handlers

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMenuOpen)
                menuPresenter.Content = null;
            else
                menuPresenter.Content = _menu;
            IsMenuOpen = !IsMenuOpen;
        }

        private void ProxiedControl_ActionRequested(object sender, RequestEventArgs e)
        {
            TypeInfo typeInfo = e.TargetElement.GetTypeInfo();
            if (typeInfo.IsSubclassOf(typeof(Page)))
            {
                PageManager.NavigateTo(e.TargetElement, null, NavigationType.Default);
                return;
            }

            if (sender as MenuControl != null)
            {
                MenuButton_Click(null, null);
                _contentControlManager.Clear();
                _titleBuilder.Clear();
            }

            if (typeInfo.IsSubclassOf(typeof(UserControl)))
            {

                _contentControlManager.NavigateToControl(e.TargetElement, e.Parameter);
                _titleBuilder.AddComponent(_contentControlManager.CurrentControl.DisplayTitle);
                SetTitleAndContent();
            }
        }

        private void ViewTodayButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsMenuOpen == true)
                MenuButton_Click(null, null);

            if (_contentControlManager.CurrentControl.GetType() == typeof(EnhancedTimetableControl))
                _contentControlManager.RefreshCurrentControl();
            else
            {
                _contentControlManager.Clear();
                _titleBuilder.Clear();
                ProxiedControl_ActionRequested(null, new RequestEventArgs(typeof(EnhancedTimetableControl), null));
            }
        }

        private void SetTitleAndContent()
        {
            contentPresenter.Content = _contentControlManager.CurrentControl;
            NotifyPropertyChanged("TitleText");
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(HelpPage), null, NavigationType.Default);
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(SettingsPage), null, NavigationType.Default);
        }

        private void AboutButton_Click(object sender, RoutedEventArgs e)
        {
            PageManager.NavigateTo(typeof(AboutPage), null, NavigationType.Default);
        }

        #endregion

        #region IAppReturnControllable Interface Implementation

        public bool AllowAppExit()
        {
            if (IsMenuOpen == true)
            {
                MenuButton_Click(null, null);
                return false;
            }
            else if (_contentControlManager.CurrentControl != null && _contentControlManager.CanGoBack)
            {
                _contentControlManager.ReturnToLastControl();
                _titleBuilder.RemoveComponent();
                SetTitleAndContent();
                return false;
            }
            else
                return true;
        }

        #endregion

    }

    class TitleBuilder
    {
        private readonly string _separator;
        private Stack<string> _titleComponents;

        public TitleBuilder(string seperator)
        {
            _titleComponents = new Stack<string>();
            _separator = seperator;
        }

        public TitleBuilder(string seperator, IEnumerable<string> components)
        {
            _titleComponents = new Stack<string>(components);
            _separator = seperator;
        }

        
        public void SetTitle(string component)
        {
            _titleComponents = new Stack<string>();
            _titleComponents.Push(component.ToUpper());
        }

        public void AddComponent(string component)
        {
            if (_titleComponents.Count > 0)
                _titleComponents.Push(_separator);
            _titleComponents.Push(component.ToUpper());
        }

        public void RemoveComponent()
        {
            _titleComponents.Pop();
            _titleComponents.Pop();
        }

        public void Clear()
        {
            _titleComponents = new Stack<string>();
        }

        public string Title
        {
            get
            {
                StringBuilder titleBuilder = new StringBuilder();
                foreach (string component in _titleComponents.Reverse())
                    titleBuilder.Append(component);
                return titleBuilder.ToString();
            }
        }

        public List<string> Components
        {
            get
            {
                List<string> components = new List<string>();
                foreach (string s in _titleComponents.Reverse())
                    components.Add(s);
                return components;
            }
        }

    }

}
