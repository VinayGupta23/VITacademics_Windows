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

    public sealed class StatusToBrushConverter : IValueConverter
    {
        public SolidColorBrush PresentBrush { get; set; }
        public SolidColorBrush AbsentBrush { get; set; }
        public SolidColorBrush OnDutyBrush { get; set; }
        public SolidColorBrush FallbackBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string status = value as string;
            if (string.Equals(status, "Present", StringComparison.OrdinalIgnoreCase))
                return PresentBrush;
            else if (string.Equals(status, "On Duty", StringComparison.OrdinalIgnoreCase))
                return OnDutyBrush;
            else if (string.Equals(status, "Absent", StringComparison.OrdinalIgnoreCase))
                return AbsentBrush;
            else
                return FallbackBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class AttendanceToForegroundConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double val = (double)value;

            if (val > 75)
                return new SolidColorBrush(Colors.Green);
            if (val == 75)
                return new SolidColorBrush(Colors.DarkOrange);
            else
                return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public sealed class DateTimeToStringConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            DateTimeOffset date = (DateTimeOffset)value;
            return date.ToString(parameter as string);
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

    public sealed class BoolToVisibilityConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value == true)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

#if WINDOWS_PHONE_APP
    public class ClassInfoTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ClassTodayTemplate { get; set; }
        public DataTemplate ClassGeneralTemplate { get; set; }
        public DataTemplate CustomInfoTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            CalendarAwareStub infoStub = item as CalendarAwareStub;

            if (infoStub is CustomInfoStub)
                return CustomInfoTemplate;

            DateTimeOffset now = DateTimeOffset.Now;
            if (infoStub.ContextDate.Date == DateTimeOffset.Now.Date)
                return ClassTodayTemplate;
            else
                return ClassGeneralTemplate;
        }
    }

    
#endif

    public class GradeToBrushConverter : IValueConverter
    {
        public SolidColorBrush FGradeBrush { get; set; }
        public SolidColorBrush NGradeBrush { get; set; }
        public SolidColorBrush WGradeBrush { get; set; }
        public SolidColorBrush SGradeBrush { get; set; }
        public SolidColorBrush DefaultBrush { get; set; }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            switch((char)value)
            {
                case 'S': return SGradeBrush;
                case 'W': return WGradeBrush;
                case 'N': return NGradeBrush;
                case 'F': return FGradeBrush;
                default: return DefaultBrush;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class CourseTemplateSelector : DataTemplateSelector
    {
        public DataTemplate CBLTemplate { get; set; }
        public DataTemplate LBCTemplate { get; set; }
        public DataTemplate PBLTemplate { get; set; }
        public DataTemplate RBLTemplate { get; set; }
        public DataTemplate PBCTemplate { get; set; }
        public DataTemplate FallbackTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item == null)
                return FallbackTemplate;

            try
            {
                Course course = item as Course;
                switch (course.CourseMode)
                {
                    case "CBL": return CBLTemplate;
                    case "LBC": return LBCTemplate;
                    case "PBL": return PBLTemplate;
                    case "RBL": return RBLTemplate;
                    case "PBC": return PBCTemplate;
                    default: throw new Exception();
                }
            }
            catch
            {
                return FallbackTemplate;
            }
        }

    }

}
