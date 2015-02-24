using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    public class ClassHours
    {
        public DateTimeOffset StartHours { get; private set; }
        public DateTimeOffset EndHours { get; private set; }
        public DayOfWeek Day { get; private set; }

        public ClassHours(DateTimeOffset startHours, DateTimeOffset endHours, DayOfWeek day)
        {
            StartHours = startHours;
            EndHours = endHours;
            Day = day;
        }
    }

    public class Attendance
    {
        public ushort TotalClasses { get; set; }
        public ushort AttendedClasses { get; set; }
        public float Percentage { get; set; }
        public SortedSet<AttendanceDetail> AttendanceDetails { get; set; }
    }

    public class AttendanceDetail
    {
        public DateTimeOffset ClassDate { get; private set; }
        public string Status { get; private set; }
        public string Reason { get; private set; }

        public AttendanceDetail(DateTimeOffset classDate, string status, string reason)
        {
            ClassDate = classDate;
            Status = status;
            Reason = reason;
        }
    }

    public class Marks
    { }

}