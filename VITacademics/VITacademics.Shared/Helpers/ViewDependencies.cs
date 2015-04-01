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
            if (status == "Present")
                return PresentBrush;
            else if (status == "On Duty")
                return OnDutyBrush;
            else if (status == "Absent")
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
                return new SolidColorBrush(Colors.Brown);
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

    public class ClassInfoTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ClassTodayTemplate { get; set; }
        public DataTemplate ClassGeneralTemplate { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            CalenderAwareInfoStub infoStub = item as CalenderAwareInfoStub;
            DateTimeOffset now = DateTimeOffset.Now;
            if (infoStub.ContextDate.Date == DateTimeOffset.Now.Date)
            {
                return ClassTodayTemplate;
            }
            else
                return ClassGeneralTemplate;
        }
    }

}
