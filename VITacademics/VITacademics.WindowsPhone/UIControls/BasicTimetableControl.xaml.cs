using Academics.DataModel;
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


namespace VITacademics.UIControls
{
    public sealed partial class BasicTimetableControl : UserControl, IProxiedControl
    {

        public BasicTimetableControl()
        {
            this.InitializeComponent();
        }

        public void GenerateView(object parameter)
        {
            Timetable timetable = parameter as Timetable;
            int j = 0;
            List<PivotItem> pivotItems = new List<PivotItem>(7);
            for (int i = 0; i < 7; i++)
            {
                var daySchedule = timetable[(DayOfWeek)i];
                if (daySchedule.Count != 0)
                {
                    pivotItems.Add(new PivotItem());
                    pivotItems[j].Header = ((DayOfWeek)i).ToString().ToLower();
                    pivotItems[j].DataContext = daySchedule;
                    j++;
                }
            }
            rootPivot.ItemsSource = pivotItems;
        }

        public event EventHandler<RequestEventArgs> ActionRequested;

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if(ActionRequested != null)
            {
                ActionRequested(this, new RequestEventArgs(typeof(UserControl), (e.ClickedItem as ClassHours).Parent));
            }
        }
    }
}
