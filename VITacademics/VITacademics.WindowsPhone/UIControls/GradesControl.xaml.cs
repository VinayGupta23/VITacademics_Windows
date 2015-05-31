using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VITacademics.Managers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Academics.DataModel;
using System.ComponentModel;
using VITacademics.Helpers;
using Windows.UI.Popups;


namespace VITacademics.UIControls
{
    public sealed partial class GradesControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {
        private AcademicHistory _academicHistory;
        public AcademicHistory GradeHistory
        {
            private set
            {
                _academicHistory = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GradeHistory"));
                    PropertyChanged(this, new PropertyChangedEventArgs("RefreshDate"));
                }
                if (value != null)
                {
                    gradeGroups.Source = value.SemesterGroups;
                }
            }
            get { return _academicHistory; }
        }

        public GradesControl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public event EventHandler<RequestEventArgs> ActionRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        public async void GenerateView(string parameter)
        {
            var response = await UserManager.GetGradesFromCacheAsync();
            if (response.Code == Academics.ContentService.StatusCode.Success)
            {
                GradeHistory = response.Content;
            }
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

            if (UserManager.IsBusy)
            {
                await new MessageDialog("Please wait while your last requested task completes.", "Busy").ShowAsync();
                return;
            }
            refreshButton.IsEnabled = false;
            refreshButton.Content = "refreshing...";

            var response = await UserManager.RequestGradesFromServerAsync();
            if (response.Code == Academics.ContentService.StatusCode.Success)
            {
                GradeHistory = response.Content;
            }
            else
            {
                await StandardMessageDialogs.GetDialog(response.Code).ShowAsync();
            }

            refreshButton.IsEnabled = true;
            refreshButton.Content = "refresh";
        }

    }
}
