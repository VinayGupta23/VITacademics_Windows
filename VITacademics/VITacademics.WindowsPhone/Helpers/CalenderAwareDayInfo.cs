using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VITacademics.Managers;


namespace VITacademics.Helpers
{

    // Pending changes.
    public class CalenderAwareInfoStub : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly DateTimeOffset _contextDate;
        private readonly ClassHours _sessionHours;
        private readonly AttendanceStub _attendanceInfo;
        private Tuple<string, string> _appointmentInfo;

        public DateTimeOffset ContextDate { get { return _contextDate; } }
        public ClassHours SessionHours { get { return _sessionHours; } }
        public AttendanceStub AttendanceInfo { get { return _attendanceInfo; } }
        public Tuple<string, string> AppointmentInfo
        {
            get { return _appointmentInfo; }
            internal set
            {
                _appointmentInfo = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("AppointmentInfo"));
            }
        }

        private CalenderAwareInfoStub(DateTimeOffset contextDate, KeyValuePair<ClassHours, AttendanceStub> infoPair)
        {
            _contextDate = contextDate;
            _sessionHours = infoPair.Key;
            _attendanceInfo = infoPair.Value;
        }
    
        internal static ObservableCollection<CalenderAwareInfoStub> GetStubCollection(DateTimeOffset contextDate, DayInfo dayInfo)
        {
            var regularClassesInfo = new ObservableCollection<CalenderAwareInfoStub>();
            foreach(var infoPair in dayInfo.RegularClassDetails)
            {
                regularClassesInfo.Add(new CalenderAwareInfoStub(contextDate, infoPair));
            }
            return regularClassesInfo;
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
            RegularClassesInfo = CalenderAwareInfoStub.GetStubCollection(contextDate, dayInfo);

            if (RegularClassesInfo.Count == 0)
                _isEmptyDay = true;
            if (ExtraClassesInfo.Count != 0)
                _hadExtraClasses = true;
        }

        public async Task LoadAppointmentsAsync()
        {
            foreach (CalenderAwareInfoStub stub in this.RegularClassesInfo)
                await CalendarManager.AssignAppointmentIfAvailableAsync(stub);
        }
    }
}
