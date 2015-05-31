using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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


namespace VITacademics.UIControls
{
    public sealed partial class MenuControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {

        public class MenuItem
        {
            public string Header
            {
                get;
                private set;
            }
            public string SubHeader
            {
                get;
                private set;
            }
            public IconElement Icon
            {
                get;
                private set;
            }

            public MenuItem(string header, string subHeader, Symbol icon)
            {
                Header = header;
                SubHeader = subHeader;
                Icon = new SymbolIcon(icon);
            }
            public MenuItem(string header, string subHeader, string iconUri)
            {
                Header = header;
                SubHeader = subHeader;
                BitmapIcon icon = new BitmapIcon();
                icon.UriSource = new Uri(iconUri);
                Icon = icon;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        public List<MenuItem> MenuItems
        {
            get;
            private set;
        }

        public MenuControl()
        {
            this.InitializeComponent();

            MenuItems = new List<MenuItem>();
            MenuItems.Add(new MenuItem("overview", "a summary of all your courses", Symbol.Globe));
            MenuItems.Add(new MenuItem("timetable", "your regular schedule of classes", "ms-appx:///Assets/Icons/TimetableButton.scale-240.png"));
            MenuItems.Add(new MenuItem("daily buzz", "watch your activity and set reminders", Symbol.Calendar));
            MenuItems.Add(new MenuItem("grades", "view grades and use the cgpa calculator", "ms-appx:///Assets/Icons/GraphButton.scale-240.png"));
            
            this.DataContext = this;
        }

        public event EventHandler<RequestEventArgs> ActionRequested;

        public void GenerateView(string parameter)
        {
        }

        private void MenuList_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (ActionRequested != null)
            {
                MenuItem item = e.ClickedItem as MenuItem;
                if (item != null)
                {
                    if (item == MenuItems[0])
                        ActionRequested(this, new RequestEventArgs(typeof(UserOverviewControl), null));
                    else if (item == MenuItems[1])
                        ActionRequested(this, new RequestEventArgs(typeof(BasicTimetableControl), null));
                    else if (item == MenuItems[2])
                        ActionRequested(this, new RequestEventArgs(typeof(EnhancedTimetableControl), null));
                    else if (item == MenuItems[3])
                        ActionRequested(this, new RequestEventArgs(typeof(GradesControl), null));
                    else
                        return;
                }
            }
        }


        public Dictionary<string, object> SaveState()
        {
            return null;
        }

        public void LoadState(Dictionary<string, object> lastState)
        {

        }
    }
}
