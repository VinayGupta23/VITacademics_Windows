using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using VITacademics.Helpers;
using VITacademics.Managers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


namespace VITacademics.UIControls
{
    public sealed partial class AdvisorControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {
        private FacultyAdvisor _advisor;

        public FacultyAdvisor Advisor
        {
            get { return _advisor; }
            set
            {
                _advisor = value;
                NotifyPropertyChanged();
            }
        }

        public AdvisorControl()
        {
            this.InitializeComponent();
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Faculty Advisor");
#endif
        }
        
        public event EventHandler<RequestEventArgs> ActionRequested;

        public string DisplayTitle
        {
            get { return "FACULTY ADVISOR"; }
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public async void LoadView(string parameter, Dictionary<string, object> lastState = null)
        {
            if (Advisor == null)
            {
                var response = await UserManager.GetAdvisorFromCacheAsync();
                if (response.Code == Academics.ContentService.StatusCode.Success)
                {
                    Advisor = response.Content;
                }
            }
        }
        
        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

            if (UserManager.IsBusy)
            {
                await new MessageDialog("Please wait while your last requested task completes.", "Busy").ShowAsync();
                return;
            }
            /*
            refreshButton.IsEnabled = false;
            refreshButton.Content = "refreshing...";
            */
            var response = await UserManager.RequestAdvisorFromServerAsync();
            if (response.Code == Academics.ContentService.StatusCode.Success)
            {
                Advisor = response.Content;
            }
            else
            {
                await StandardMessageDialogs.GetDialog(response.Code).ShowAsync();
            }
            /*
            refreshButton.IsEnabled = true;
            refreshButton.Content = "refresh";
            */
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
