using System;


namespace Academics.DataModel
{
   
    public class CoursesMetadata
    {
        private readonly string _semester;
        private readonly DateTimeOffset _refreshedDate;

        public string Semester
        {
            get
            { return _semester; }
        }
        public DateTimeOffset RefreshedDate
        {
            get
            { return _refreshedDate; }
        }

        public CoursesMetadata(string semester, DateTimeOffset refreshedDate)
        {
            _semester = semester;
            _refreshedDate = refreshedDate;
        }

    }

    public class ClassHours : LtpCourseComponent
    {
        private readonly DateTimeOffset _startHours;
        private readonly DateTimeOffset _endHours;
        private readonly DayOfWeek _day;

        public DateTimeOffset StartHours
        {
            get
            { return _startHours; }
        }
        public DateTimeOffset EndHours
        {
            get
            { return _endHours; }
        }
        public DayOfWeek Day
        {
            get
            { return _day; }
        }

        public ClassHours(LtpCourse course, DateTimeOffset startHours, DateTimeOffset endHours, DayOfWeek day)
            : base(course)
        {
            _startHours = startHours;
            _endHours = endHours;
            _day = day;
        }
    }

    public class MarksInfo : LtpCourseComponent
    {
        private readonly double? _marks;
        private readonly string _status;
        
        public double? Marks
        {
            get
            { return _marks; }
        }
        public string Status
        {
            get
            { return _status; }
        }

        public MarksInfo(LtpCourse parent, double? marks, string status)
            : base(parent)
        {
            _marks = marks;
            _status = status;
        }
    }

}