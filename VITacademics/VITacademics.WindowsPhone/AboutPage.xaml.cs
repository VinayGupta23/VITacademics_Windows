using System;
using System.Collections.Generic;
using VITacademics.Managers;
using Windows.ApplicationModel.Email;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Academics.SystemMetadata;
using Academics.ContentService;
using Windows.UI.Xaml.Documents;
using Windows.System;


namespace VITacademics
{

    public sealed partial class AboutPage : Page, IManageable, INotifyPropertyChanged
    {
        private StatusBar _statusBar;
        private List<Contributor> _contributors = null;
        private Visibility _progressIndicatorVisibility = Visibility.Collapsed;
        private Visibility _statusVisibility = Visibility.Collapsed;

        public List<Contributor> Contributors
        {
            get { return _contributors; }
            set
            {
                _contributors = value;
                NotifyPropertyChanged();
            }
        }
        public Visibility ProgressIndicatorVisiblity
        {
            get { return _progressIndicatorVisibility; }
            set
            {
                _progressIndicatorVisibility = value;
                NotifyPropertyChanged();
            }
        }
        public Visibility StatusVisibility
        {
            get { return _statusVisibility; }
            set
            {
                _statusVisibility = value;
                NotifyPropertyChanged();
            }
        }
        public string AppVersion
        {
            get;
            private set;
        }

        public AboutPage()
        {
            this.InitializeComponent();
            _statusBar = StatusBar.GetForCurrentView();
            AppVersion = MetadataProvider.AppVersion;
            this.DataContext = this;

#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendView("About Page");
#endif
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

        #region Interface Implementations

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public async void LoadState(Dictionary<string, object> lastState)
        {
            ProgressIndicatorVisiblity = Visibility.Visible;

            var response = await MetadataProvider.GetContributesFromCacheAsync();
            if (response.Code == StatusCode.Success)
                Contributors = response.Content;

            await Task.Run(async () =>
            { response = await MetadataProvider.RequestContributorsFromSystemAsync(); });

            if (response.Code == StatusCode.Success)
                Contributors = response.Content;
            else
            {
                if (Contributors == null)
                    StatusVisibility = Visibility.Visible;
            }

            ProgressIndicatorVisiblity = Visibility.Collapsed;
        }

        #endregion

        private void FeedbackButton_Click(Hyperlink sender, HyperlinkClickEventArgs args)
        {
            PageManager.NavigateTo(typeof(SettingsPage), "feedback", NavigationType.Default);
        }

        private async void ContributorList_ItemClick(object sender, ItemClickEventArgs e)
        {
            Contributor current = e.ClickedItem as Contributor;
            await Launcher.LaunchUriAsync(new Uri(current.GithubProfileUri));
        }
    }
}
