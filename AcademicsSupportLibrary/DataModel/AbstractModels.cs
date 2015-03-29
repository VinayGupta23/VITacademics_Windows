using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Academics.DataModel
{
    public abstract class LtpCourseComponent
    {
        private LtpCourse _parent;
        public LtpCourse Parent
        {
            get { return _parent; }
        }
        
        public LtpCourseComponent(LtpCourse parent)
        {
            _parent = parent;
        }
    }

    public abstract class Course
    {
        public ushort ClassNumber { get; internal set; }
        public string CourseCode { get; internal set; }
        public string Title { get; internal set; }
        public string CourseMode { get; internal set; }
        public string CourseOption { get; internal set; }
        public string SubjectType { get; internal set; }
        public string Faculty { get; internal set; }
    }

    public abstract class LtpCourse : Course
    {
        private List<ClassHours> _timings;

        public string Slot { get; internal set; }
        public string Venue { get; internal set; }
        public string Ltpc { get; internal set; }
        public double InternalMarksScored { get; internal set; }
        public double TotalMarksTested { get; internal set; }
        public ReadOnlyCollection<ClassHours> Timings { get; private set; }
        public Attendance Attendance { get; internal set; }

        public LtpCourse()
        {
            _timings = new List<ClassHours>();
            Timings = new ReadOnlyCollection<ClassHours>(_timings);
        }

        internal void AddClassHoursInstance(ClassHours classHours)
        {
            _timings.Add(classHours);
        }
    }

    public abstract class NonLtpCourse : Course
    {
        public string Credits { get; internal set; }
    }
}
