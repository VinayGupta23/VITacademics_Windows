using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    public class ClassHours
    {
        public DateTimeOffset StartHours { get; set; }
        public DateTimeOffset EndHours { get; set; }
        public DayOfWeek Day { get; set; }
    }

    public class Attendance
    {
        // Array of AttendanceDetail
    }

    public class AttendanceDetail
    {
        public DateTimeOffset ClassDate { get; set; }
        public string Status { get; set; }
        public string Reason { get; set; }
    }

    public class Marks
    {

    }

}