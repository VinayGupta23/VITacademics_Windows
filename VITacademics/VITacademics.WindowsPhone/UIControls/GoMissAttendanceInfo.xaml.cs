using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace VITacademics.UIControls
{
    public sealed partial class GoMissAttendanceInfo : UserControl
    {
        public int Percentage
        {
            get;
            private set;
        }
        public int GoPercentage
        {
            get;
            private set;
        }
        public int MissPercentage
        {
            get;
            private set;
        }

        public GoMissAttendanceInfo()
        {
            this.InitializeComponent();
            this.DataContextChanged += GoMissAttendanceInfo_DataContextChanged;
        }

        private void GoMissAttendanceInfo_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (this.DataContext == null)
                return;

            Attendance a = this.DataContext as Attendance;
            Percentage = (int)a.Percentage;
            GoPercentage = (int)Math.Ceiling((double)(a.AttendedClasses + 1) * 100.00 / (a.TotalClasses + 1));
            MissPercentage = (int)Math.Ceiling((double)a.AttendedClasses * 100.00 / (a.TotalClasses + 1));

            infoGrid.DataContext = this;
        }
    }
}
