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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace VITacademics.UIControls
{
    public sealed partial class EnhancedTimetableControl : UserControl, IProxiedControl
    {

        public EnhancedTimetableControl()
        {
            this.InitializeComponent();
        }

        /*
        private void Pivot_PivotItemLoading(Pivot sender, PivotItemEventArgs args)
        {
            int curIndex = sender.SelectedIndex;
            for (int i = curIndex + 1; i < curIndex + 4; i++)
            {
                _dates[i % 5] = _dates[curIndex].AddDays(i - curIndex);
            }
            int previousIndex = (curIndex + 4) % 5;
            _dates[previousIndex] = _dates[curIndex].AddDays(-1);
            for (int i = 0; i < 5; i++)
            {
                (sender.Items[i] as PivotItem).Header = _dates[i].ToString("ddd dd");
            }

            if (_dates[curIndex].Date == DateTime.Now.Date)
                args.Item.Content = new CurrentDayControl();
            
        }
        
       
       
       
        */

        public event EventHandler<RequestEventArgs> ActionRequested;

        public void GenerateView(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
