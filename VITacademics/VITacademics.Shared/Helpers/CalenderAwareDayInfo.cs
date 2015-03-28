using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VITacademics.Helpers
{

    public class CalenderAwareInfoStub : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DateTimeOffset _contextDate;
        private readonly ClassHours _sessionHours;
        private readonly AttendanceStub _attendanceInfo;

        public DateTimeOffset ContextDate { get { return _contextDate; } }
        public ClassHours SessionHours { get { return _sessionHours; } }
        public AttendanceStub AttendanceInfo { get { return _attendanceInfo; } }

        public CalenderAwareInfoStub(DateTimeOffset contextDate, KeyValuePair<ClassHours, AttendanceStub> infoPair)
        {
            _contextDate = contextDate;
            _sessionHours = infoPair.Key;
            _attendanceInfo = infoPair.Value;
        }
    }

    public class CalenderAwareDayInfo
    {
        private readonly bool _isEmptyDay;
        private readonly bool _hadExtraClasses;
        
        public ObservableCollection<CalenderAwareInfoStub> RegularClassesInfo
        {
            get;
            private set;
        }
        public List<KeyValuePair<LtpCourse, AttendanceStub>> ExtraClassesInfo
        {
            get;
            private set;
        }
        public bool IsEmptyDay
        {
            get { return _isEmptyDay; }
        }
        public bool HadExtraClasses
        {
            get { return _hadExtraClasses; }
        }

        public CalenderAwareDayInfo(DateTimeOffset contextDate, DayInfo dayInfo)
        {
            ExtraClassesInfo = dayInfo.ExtraClassDetails;
            RegularClassesInfo = new ObservableCollection<CalenderAwareInfoStub>();
            foreach (var infoPair in dayInfo.RegularClassDetails)
                RegularClassesInfo.Add(new CalenderAwareInfoStub(contextDate, infoPair));

            if (RegularClassesInfo.Count == 0)
                _isEmptyDay = true;
            if (ExtraClassesInfo.Count != 0)
                _hadExtraClasses = true;
        }

    }
}
