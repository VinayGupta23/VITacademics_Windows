using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Academics.DataModel
{

    public class Timetable
    {
        private List<ClassHours>[] _weekSchedule = new List<ClassHours>[7];

        private Timetable()
        {
            for (int i = 0; i < _weekSchedule.Length; i++)
                _weekSchedule[i] = new List<ClassHours>();
        }

        public ReadOnlyCollection<ClassHours> this[DayOfWeek day]
        {
            get
            {
                return new ReadOnlyCollection<ClassHours>(_weekSchedule[(int)day]);
            }
        }

        public static Timetable GetTimetable(IEnumerable<Course> courses)
        {
            try
            {
                Timetable timetable = new Timetable();
                foreach (Course c in courses)
                {
                    LtpCourse course = c as LtpCourse;
                    if (course == null)
                        continue;

                    foreach (ClassHours classHours in course.Timings)
                        timetable._weekSchedule[(int)classHours.Day].Add(classHours);
                }
                foreach (var daySchedule in timetable._weekSchedule)
                    daySchedule.Sort(ClassHoursComparision);

                return timetable;
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Tuple<ClassHours, AttendanceStub>> GetDayInfo(DateTimeOffset instructionDayDate)
        {
            int day = (int)instructionDayDate.DayOfWeek;
            int itemCount = _weekSchedule[day].Count;

            List<Tuple<ClassHours, AttendanceStub>> dayInfo = new List<Tuple<ClassHours, AttendanceStub>>(itemCount);
            for (int i = 0; i < itemCount; i++)
            {
                AttendanceStub stub;
                _weekSchedule[day][i].Parent.Attendance.Details.TryGetValue(instructionDayDate, out stub);
                dayInfo[i] = new Tuple<ClassHours, AttendanceStub>(_weekSchedule[day][i], stub);
            }

            return dayInfo;
        }

        private static int ClassHoursComparision(ClassHours x, ClassHours y)
        {
            return DateTimeOffset.Compare(x.StartHours, y.StartHours);
        }

    }

}
