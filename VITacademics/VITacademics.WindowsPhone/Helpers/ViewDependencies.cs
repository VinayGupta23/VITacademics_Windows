using Academics.DataModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace VITacademics.Helpers
{
    public sealed class StatusToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string status = value as string;
            if (status == "Present")
                return new SolidColorBrush(Colors.Green);
            else if (status == "On Duty")
                return new SolidColorBrush(Colors.DeepSkyBlue);
            else if (status == "Absent")
                return new SolidColorBrush(Colors.Red);
            else
                return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return "";
        }
    }

    public sealed class StatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string status = value as string;
            if (status == "Present")
                return new SolidColorBrush(Colors.LightGreen);
            else if (status == "Absent")
                return new SolidColorBrush(Colors.LightSalmon);
            else
                return new SolidColorBrush(Colors.LightGray);
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }
    }

    public sealed class NullableDateToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return parameter as string;
            else
                return ((DateTimeOffset)value).ToString("dd MMM, yyyy");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class EnumerableToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (!(value as IEnumerable).GetEnumerator().MoveNext())
            {
                return Visibility.Visible;
            }
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
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
