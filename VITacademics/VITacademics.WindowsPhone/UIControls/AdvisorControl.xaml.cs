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
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.Email;
using Windows.ApplicationModel.Calls;
using Windows.ApplicationModel.Chat;

namespace VITacademics.UIControls
{
    public sealed partial class AdvisorControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {

        private Visibility _detailsVisibility = Visibility.Collapsed;
        private string _refreshMessage;
        private FacultyAdvisor _advisor;
        private List<Tuple<string, string>> _contactMenuItems;

        public Visibility DetailsVisibility
        {
            get { return _detailsVisibility; }
            private set
            {
                _detailsVisibility = value;
                NotifyPropertyChanged();
            }
        }
        public string RefreshMessage
        {
            get { return _refreshMessage; }
            private set
            {
                _refreshMessage = value;
                NotifyPropertyChanged();
            }
        }
        public FacultyAdvisor Advisor
        {
            get { return _advisor; }
            private set
            {
                _advisor = value;
                NotifyPropertyChanged();
            }
        }
        public List<Tuple<string, string>> ContactMenuItems
        {
            get { return _contactMenuItems; }
            set
            {
                _contactMenuItems = value;
                NotifyPropertyChanged();
            }
        }

        public AdvisorControl()
        {
            this.InitializeComponent();
            this.DataContext = this;

            RefreshMessage = "Faculty advisor details have not yet been downloaded. Download now?";
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendView("Faculty Advisor");
#endif
        }

        #region Interface Implementations

        public event EventHandler<RequestEventArgs> ActionRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

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
                    ShowAdvisorDetails(response.Content);
                    AlterRefreshViewForFirstSuccess();
                }
            }
        }

        #endregion

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (UserManager.IsBusy)
            {
                await new MessageDialog("Please wait while your last requested task completes.", "Busy").ShowAsync();
                return;
            }

            refreshButton.Content = "Refreshing...";
            refreshButton.IsEnabled = false;

            var response = await UserManager.RequestAdvisorFromServerAsync();
            if (response.Code == Academics.ContentService.StatusCode.Success)
            {
                if (Advisor == null)
                {
                    AlterRefreshViewForFirstSuccess();
                }
                else
                {
                    refreshButton.Content = "refresh complete";
                    refreshButton.IsEnabled = false;
                }
                ShowAdvisorDetails(response.Content);
            }
            else
            {
                await StandardMessageDialogs.GetDialog(response.Code).ShowAsync();
                refreshButton.Content = "refresh again";
                refreshButton.IsEnabled = true;
            }
        }

        private async void ContactList_ItemClick(object sender, ItemClickEventArgs e)
        {
            var item = e.ClickedItem as Tuple<string, string>;

            if(item == ContactMenuItems[0])
            {
                PhoneCallManager.ShowPhoneCallUI(Advisor.Phone, Advisor.Name);
            }
            else if(item == ContactMenuItems[1])
            {
                ChatMessage msg = new ChatMessage();
                msg.Recipients.Add(Advisor.Phone);
                await ChatMessageManager.ShowComposeSmsMessageAsync(msg);
            }
            else if (item == ContactMenuItems[2])
            {
                EmailMessage mailMsg = new EmailMessage();
                mailMsg.Subject = UserManager.CurrentUser.RegNo + " - Faculty Advisor Student";
                mailMsg.To.Add(new EmailRecipient(Advisor.Email, Advisor.Name));
                await EmailManager.ShowComposeNewEmailAsync(mailMsg);
            }

        }

        private void ShowAdvisorDetails(FacultyAdvisor advisor)
        {
            Advisor = advisor;
            var contactMenuItems = new List<Tuple<string, string>>(3);
            contactMenuItems.Add(new Tuple<string, string>("call mobile", Advisor.Phone));
            contactMenuItems.Add(new Tuple<string, string>("text", "send SMS"));
            contactMenuItems.Add(new Tuple<string, string>("send email", Advisor.Email));
            ContactMenuItems = contactMenuItems;
            DetailsVisibility = Visibility.Visible;
        }

        private void AlterRefreshViewForFirstSuccess()
        {
            RefreshMessage = "Details showing incorrectly or advisor changed?";
            refreshButton.Content = "refresh details";
            refreshButton.IsEnabled = true;
        }
    }
}
