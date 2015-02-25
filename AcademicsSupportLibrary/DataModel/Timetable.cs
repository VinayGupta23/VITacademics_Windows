using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    public class Timetable
    {
        private Dictionary<DayOfWeek, List<Tuple<ClassHours, LtpCourse>>> _weekTimetable;

        public IReadOnlyList<Tuple<ClassHours, LtpCourse>> this[DayOfWeek day]
        {
            get
            {
                try
                {
                    return _weekTimetable[day];
                }
                catch
                {
                    return null;
                }
            }
        }

        public void StartNewBatch()
        {
            _weekTimetable = new Dictionary<DayOfWeek, List<Tuple<ClassHours, LtpCourse>>>();
        }

        public void AddTimingsToBatch(LtpCourse source)
        {
            foreach (ClassHours classHours in source.CourseTimings)
            {
                DayOfWeek day = classHours.Day;

                if (_weekTimetable.ContainsKey(day) == false)
                    _weekTimetable[day] = new List<Tuple<ClassHours, LtpCourse>>();

                _weekTimetable[day].Add(new Tuple<ClassHours, LtpCourse>(classHours, source));
            }
        }

        public void FinalizeBatch()
        {
            foreach (List<Tuple<ClassHours, LtpCourse>> dayTimetable in _weekTimetable.Values)
            {
                dayTimetable.Sort(
                    (Tuple<ClassHours, LtpCourse> A, Tuple<ClassHours, LtpCourse> B)
                        => DateTimeOffset.Compare(A.Item1.StartHours, B.Item1.StartHours)
                                );
            }
        }
    }
}
