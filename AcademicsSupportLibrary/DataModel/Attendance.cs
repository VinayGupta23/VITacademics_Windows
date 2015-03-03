using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Academics.DataModel
{
    public class Attendance : LtpCourseComponent
    {
        internal SortedDictionary<DateTimeOffset, AttendanceStub> _details;

        public ushort TotalClasses { get; private set; }
        public ushort AttendedClasses { get; private set; }
        public double Percentage { get; private set; }
        public ReadOnlyDictionary<DateTimeOffset, AttendanceStub> Details { get; private set; }

        public Attendance(LtpCourse course, ushort totalClasses, ushort attendedClasses, double percentage)
            : base(course)
        {
            TotalClasses = totalClasses;
            AttendedClasses = attendedClasses;
            Percentage = percentage;

            _details = new SortedDictionary<DateTimeOffset, AttendanceStub>();
            Details = new ReadOnlyDictionary<DateTimeOffset, AttendanceStub>(_details);
        }

        internal void AddAttendanceStub(DateTimeOffset classDate, string status, string reason)
        {
            try
            {
                _details.Add(classDate, new AttendanceStub(status, reason));
            }
            catch { }
        }
    }

    public class AttendanceStub
    {
        private readonly string _status;
        private readonly string _reason;

        public string Status
        {
            get { return _status; }
        }
        public string Reason
        {
            get { return _reason; }
        }

        public AttendanceStub(string status, string reason)
        {
            _status = status;
            _reason = reason;
        }
    }
}
