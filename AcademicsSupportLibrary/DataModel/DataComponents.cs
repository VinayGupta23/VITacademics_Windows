using System;


namespace Academics.DataModel
{
   
    public class CoursesMetadata
    {
        private readonly string _semester;
        private readonly DateTimeOffset _refreshedDate;
        private readonly ushort _totalCredits;

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
        public ushort TotalCredits
        {
            get
            { return _totalCredits; }
        }

        public CoursesMetadata(string semester, DateTimeOffset refreshedDate, ushort totalCredits)
        {
            _semester = semester;
            _refreshedDate = refreshedDate;
            _totalCredits = totalCredits;
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

    public class MarkInfo : LtpCourseComponent
    {
        private readonly string _title;
        private readonly int _maxMarks;
        private readonly int _weightage;
        private readonly DateTimeOffset? _conductedDate;
        private readonly double? _marks;
        private readonly double? _weightedMarks;
        private readonly string _status;

        public string Title
        {
            get
            { return _title; }
        }
        public int MaxMarks
        {
            get
            { return _maxMarks; }
        }
        public int Weightage
        {
            get
            { return _weightage; }
        }
        public DateTimeOffset? ConductedDate
        {
            get
            { return _conductedDate; }
        }
        public double? Marks
        {
            get
            { return _marks; }
        }
        public double? WeightedMarks
        {
            get { return _weightedMarks; }
        }
        public string Status
        {
            get
            { return _status; }
        }

        public MarkInfo(LtpCourse parent, string marksTitle, int maxMarks, int weightage, DateTimeOffset? conductedDate, double? marks, string status)
            : base(parent)
        {
            _title = marksTitle;
            _maxMarks = maxMarks;
            _weightage = weightage;
            _conductedDate = conductedDate;
            _marks = marks;
            _status = status;

            if (_marks != null)
                _weightedMarks = Math.Round((double)_marks * _weightage / _maxMarks, 2);
        }

    }

}