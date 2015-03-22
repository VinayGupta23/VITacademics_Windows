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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace VITacademics.UIControls
{
    public sealed partial class CourseInfoControl : UserControl, IProxiedControl
    {
        public event EventHandler<RequestEventArgs> ActionRequested;

        public CourseInfoControl()
        {
            this.InitializeComponent();
        }

        public void GenerateView(object parameter)
        {
            try
            {
                ContentPresenter contentPresenter = new ContentPresenter();
                Type contentType = parameter.GetType();
                if (contentType == typeof(CBLCourse))
                    contentPresenter.ContentTemplate = this.Resources["CBLPivotTemplate"] as DataTemplate;
                else if (contentType == typeof(PBLCourse))
                    contentPresenter.ContentTemplate = this.Resources["PBLPivotTemplate"] as DataTemplate;
                else if (contentType == typeof(LBCCourse))
                    contentPresenter.ContentTemplate = this.Resources["LBCPivotTemplate"] as DataTemplate;
                else if (contentType == typeof(PBCCourse))
                    contentPresenter.ContentTemplate = this.Resources["PBCPivotTemplate"] as DataTemplate;
                else
                    contentPresenter.ContentTemplate = this.Resources["CBLPivotTemplate"] as DataTemplate;
                contentPresenter.Content = parameter;
                this.Content = contentPresenter;
            }
            catch { }
        }

    }

}
