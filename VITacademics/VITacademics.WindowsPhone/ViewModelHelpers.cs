using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace VITacademics
{
    public sealed class AttendanceStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string status = value as string;
            if (status == "Absent")
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
}
