using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Academics.DataModel
{
    public class Attendance : LtpCourseComponent
    {
        internal AttendanceDetails _details;

        public ushort TotalClasses { get; private set; }
        public ushort AttendedClasses { get; private set; }
        public double Percentage { get; private set; }
        public ReadOnlyCollection<AttendanceGroup> Details { get; private set; }
        public int SingleClassLength { get; private set; }

        public Attendance(LtpCourse course, ushort totalClasses, ushort attendedClasses, double percentage, int singleClassLength)
            : base(course)
        {
            TotalClasses = totalClasses;
            AttendedClasses = attendedClasses;
            Percentage = percentage;
            SingleClassLength = singleClassLength;

            _details = new AttendanceDetails();
            Details = new ReadOnlyCollection<AttendanceGroup>(_details);
        }

        internal void AddStubToDetails(DateTimeOffset contextDate, AttendanceStub stub)
        {
            try
            {
                if (_details.Contains(contextDate) == false)
                    _details.Add(new AttendanceGroup(contextDate));

                _details[contextDate].AddStub(stub);
            }
            catch { }
        }
    }

    public class AttendanceDetails : KeyedCollection<DateTimeOffset, AttendanceGroup>
    {
        protected override DateTimeOffset GetKeyForItem(AttendanceGroup item)
        {
            return item.ContextDate;
        }
    }

    public class AttendanceStub
    {
        private readonly string _classSlot;
        private readonly string _status;
        private readonly string _reason;

        public string ClassSlot
        {
            get { return _classSlot; }
        }
        public string Status
        {
            get { return _status; }
        }
        public string Reason
        {
            get { return _reason; }
        }

        public AttendanceStub(string classSlot, string status, string reason)
        {
            _classSlot = classSlot;
            _status = status;
            _reason = reason;
        }
    }

    public class AttendanceGroup
    {
        private List<AttendanceStub> _details;
        private DateTimeOffset _contextDate;

        public DateTimeOffset ContextDate
        {
            get { return _contextDate; }
        }
        public ReadOnlyCollection<AttendanceStub> Details
        { get; private set; }

        public AttendanceGroup(DateTimeOffset contextDate)
        {
            _details = new List<AttendanceStub>();
            Details = new ReadOnlyCollection<AttendanceStub>(_details);
            _contextDate = contextDate;
        }

        internal void AddStub(AttendanceStub stub)
        {
            _details.Add(stub);
        }
    }
}
