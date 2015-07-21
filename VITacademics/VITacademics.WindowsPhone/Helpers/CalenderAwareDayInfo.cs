using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VITacademics.Managers;
using Windows.ApplicationModel.Appointments;


namespace VITacademics.Helpers
{

    public class AppointmentInfo
    {
        private readonly string _localId;
        private readonly string _subject;
        private readonly TimeSpan _reminder;

        public string LocalId
        { get { return _localId; } }
        public string Subject
        { get { return _subject; } }
        public TimeSpan Reminder
        { get { return _reminder; } }

        public AppointmentInfo(Appointment appt)
        {
            _localId = appt.LocalId;
            _subject = CalendarManager.ExtractComponentsFromSubject(appt.Subject).Value;
            _reminder = (TimeSpan)appt.Reminder;
        }
    }

    public abstract class CalendarAwareStub : INotifyPropertyChanged
    {
        private readonly DateTimeOffset _contextDate;
        private AppointmentInfo _apptInfo;
        private Course _contextCourse;

        public event PropertyChangedEventHandler PropertyChanged;

        public DateTimeOffset ContextDate { get { return _contextDate; } }
        public Course ContextCourse { get { return _contextCourse; } } 
        public AppointmentInfo ApptInfo
        {
            get { return _apptInfo; }
            internal set
            {
                _apptInfo = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("ApptInfo"));
            }
        }

        public abstract TimeSpan StartTime { get; }
        public abstract TimeSpan EndTime { get; }

        public CalendarAwareStub(DateTimeOffset contextDate, Course contextCourse, Appointment appt)
        {
            _contextDate = contextDate;
            _contextCourse = contextCourse;
            if (appt != null)
                ApptInfo = new AppointmentInfo(appt);
        }
    }

    public class RegularInfoStub : CalendarAwareStub
    {
        private ClassHours _sessionHours;
        private AttendanceStub _attendanceInfo;

        public ClassHours SessionHours { get { return _sessionHours; } }
        public AttendanceStub AttendanceInfo { get { return _attendanceInfo; } }

        public RegularInfoStub(DateTimeOffset contextDate, ClassHours sessionHours, AttendanceStub attendanceStub, Appointment appt)
            : base(contextDate, sessionHours.Parent, appt)
        {
            _sessionHours = sessionHours;
            _attendanceInfo = attendanceStub;
        }

        public override TimeSpan StartTime
        {
            get { return SessionHours.StartHours.TimeOfDay; }
        }
        public override TimeSpan EndTime
        {
            get { return SessionHours.EndHours.TimeOfDay; }
        }
    }

    public class CustomInfoStub : CalendarAwareStub
    {
        private TimeSpan _startTime;
        private TimeSpan _endTime;

        public override TimeSpan StartTime { get { return _startTime; } }
        public override TimeSpan EndTime { get { return _endTime; } }

        public CustomInfoStub(DateTimeOffset contextDate, Appointment customAppt)
            : base(contextDate, UserManager.CurrentUser.Courses.First((c) => c.CourseCode == customAppt.Subject.Substring(0, 6)), customAppt)
        {
            if (customAppt != null)
            {
                _startTime = customAppt.StartTime.TimeOfDay;
                _endTime = customAppt.StartTime.Add(customAppt.Duration).TimeOfDay;
            }
        }
    }

    public class CalendarAwareDayInfo
    {
        private readonly DateTimeOffset _contextDate;
        private readonly bool _isEmptyDay;
        private readonly bool _hadExtraClasses;

        public DateTimeOffset ContextDate { get { return _contextDate; } }
        public ObservableCollection<CalendarAwareStub> RegularClassesInfo
        { get; set; }
        public List<KeyValuePair<LtpCourse, AttendanceStub>> ExtraClassesInfo
        { get; set; }

        public bool IsEmptyDay
        { get { return _isEmptyDay; } }
        public bool HadExtraClasses
        { get { return _hadExtraClasses; } }

        public CalendarAwareDayInfo(DateTimeOffset contextDate)
        {
            _contextDate = contextDate;
            DayInfo dayInfo = UserManager.GetCurrentTimetable().GetExactDayInfo(contextDate);
            RegularClassesInfo = new ObservableCollection<CalendarAwareStub>();
            foreach (var stub in dayInfo.RegularClassDetails)
                RegularClassesInfo.Add(new RegularInfoStub(contextDate, stub.Key, stub.Value, null));
            ExtraClassesInfo = dayInfo.ExtraClassDetails;

            if (RegularClassesInfo.Count == 0)
                _isEmptyDay = true;
            if (ExtraClassesInfo.Count != 0)
                _hadExtraClasses = true;
        }
    }

}
