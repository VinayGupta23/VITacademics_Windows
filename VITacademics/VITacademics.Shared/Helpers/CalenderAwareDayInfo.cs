using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VITacademics.Helpers
{

    public class CalenderAwareInfoStub
    {
        private readonly DateTimeOffset _contextDate;
        private readonly ClassHours _sessionHours;
        private readonly AttendanceStub _attendanceInfo;

        public DateTimeOffset ContextDate { get { return _contextDate; } }
        public ClassHours SessionHours { get { return _sessionHours; } }
        public AttendanceStub AttendanceInfo { get { return _attendanceInfo; } }

        public CalenderAwareInfoStub(DateTimeOffset contextDate, Tuple<ClassHours, AttendanceStub> tuple)
        {
            _contextDate = contextDate;
            _sessionHours = tuple.Item1;
            _attendanceInfo = tuple.Item2;
        }
    }

    public class CalenderAwareDayInfo
    {

    }
}
