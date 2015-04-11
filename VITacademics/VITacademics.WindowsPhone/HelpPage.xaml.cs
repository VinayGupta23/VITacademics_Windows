using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using VITacademics.Managers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace VITacademics
{

    public sealed partial class HelpPage : Page, IManageable
    {

        public HelpPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            PageManager.RegisterPage(this);
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
