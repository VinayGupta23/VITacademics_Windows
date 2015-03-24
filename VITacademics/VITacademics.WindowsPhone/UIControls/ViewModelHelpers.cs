using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace VITacademics.UIControls
{
    public sealed class AttendanceStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string status = value as string;
            if (status == null)
                return new SolidColorBrush(Colors.LightGray);
            else if (status == "Absent")
                return new SolidColorBrush(Colors.Red);
            else
                return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }
    }

    public sealed class DateToDateStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTimeOffset date = ((DateTimeOffset)value);
            return date.ToString("dd-MMM-yy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return default(DateTimeOffset);
        }
    }

    public sealed class DateToDayStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTimeOffset date = (DateTimeOffset)value;
            return date.ToString("dddd");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    
    public sealed class DateToTimeStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTimeOffset date = ((DateTimeOffset)value);
            return date.ToString("HH:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return default(DateTimeOffset);
        }
    }

    public class CourseTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CBLTemplate { get; set; }
        public DataTemplate PBLTemplate { get; set; }
        public DataTemplate LBCTemplate { get; set; }
        public DataTemplate PBCTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            Type contentType = (container as FrameworkElement).DataContext.GetType();
            if (contentType == typeof(CBLCourse))
                return CBLTemplate;
            else if (contentType == typeof(PBLCourse))
                return PBLTemplate;
            else if (contentType == typeof(LBCCourse))
                return LBCTemplate;
            else if (contentType == typeof(PBCCourse))
                return PBCTemplate;
            else
                return null;
        }
    }
}
