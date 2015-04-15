using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VITacademics.Managers;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace VITacademics
{

    public sealed partial class HelpPage : Page, IManageable
    {
        private StatusBar _statusBar;

        public HelpPage()
        {
            this.InitializeComponent();
            _statusBar = StatusBar.GetForCurrentView();
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

        private void FlipView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pageNoTextBlock != null)
                pageNoTextBlock.Text =
                    String.Format("page {0} of {1}", flipView.SelectedIndex + 1, flipView.Items.Count);
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {
            FlipView_SelectionChanged(null, null);
        }
    }

    public sealed class HelpItem
    {
        public string Title { get; set; }
        public ImageSource ImagePath { get; set; }
        public string Index { get; set; }
        public string ContentText { get; set; }
        public bool HasIndex { get; set; }
    }

    public sealed class HelpItemCollection : ObservableCollection<HelpItem>
    { }

}
