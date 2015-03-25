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

        public override string ToString()
        {
            return String.Format("{0} to {1}",
                                 this.StartHours.ToString("HH:mm"),
                                 this.EndHours.ToString("HH:mm"));
        }
    }

    public class MarksInfo : LtpCourseComponent
    {
        private readonly double? _marks;
        private readonly string _status;
        private readonly string _title;

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
        public string Title
        {
            get
            { return _title; }
        }

        public MarksInfo(LtpCourse parent, string title, double? marks, string status)
            : base(parent)
        {
            _title = title;
            _marks = marks;
            _status = status;
        }
    }

}