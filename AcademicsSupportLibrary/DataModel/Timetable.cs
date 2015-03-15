using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Academics.DataModel
{
    /// <summary>
    /// A timetable view of courses which provides easy access to day wise information such as class schedule and attendance.
    /// </summary>
    public class Timetable
    {

        #region Private Fields and Helper Methods

        private readonly List<ClassHours>[] _weekRegularClasses = new List<ClassHours>[7];
        private readonly List<LtpCourse>[] _weekNeglectedCourses = new List<LtpCourse>[7];

        private static int ClassHoursComparision(ClassHours x, ClassHours y)
        {
            return DateTimeOffset.Compare(x.StartHours, y.StartHours);
        }

        #endregion

        #region Constructor

        private Timetable()
        {
            for (int i = 0; i < 7; i++)
            {
                _weekRegularClasses[i] = new List<ClassHours>();
                _weekNeglectedCourses[i] = new List<LtpCourse>();
            }
        }

        #endregion

        #region Indexer and Public Methods (API)

        /// <summary>
        /// Returns the class schedule for the requested day of the week.
        /// </summary>
        /// <param name="day"></param>
        /// <returns></returns>
        public ReadOnlyCollection<ClassHours> this[DayOfWeek day]
        {
            get
            {
                return new ReadOnlyCollection<ClassHours>(_weekRegularClasses[(int)day]);
            }
        }

        /// <summary>
        /// Constructs the week's timetable for the specified courses and returns the timetable object on success.
        /// </summary>
        /// <param name="courses">
        /// The list of courses for which to generate the timetable.
        /// </param>
        /// <returns></returns>
        public static Timetable GetTimetable(IEnumerable<Course> courses)
        {
            try
            {
                Timetable timetable = new Timetable();
                
                foreach (Course c in courses)
                {
                    LtpCourse course = c as LtpCourse;
                    if (course != null)
                    {
                        BitArray punchCard = new BitArray(7);
                        // Adding the schedule of regular classes of the current courses.
                        foreach (ClassHours classHours in course.Timings)
                        {
                            int i = (int)classHours.Day;
                            timetable._weekRegularClasses[i].Add(classHours);
                            punchCard[i] = true;
                        }
                        // Adding the course to those days which do not have a regular class of the current course.
                        for (int i = 0; i < 7; i++)
                        {
                            if (punchCard[i] == false)
                                timetable._weekNeglectedCourses[i].Add(course);
                        }
                    }
                }
                foreach (var daySchedule in timetable._weekRegularClasses)
                    daySchedule.Sort(ClassHoursComparision);

                return timetable;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets complete information about an instruction day in terms of regular and extra classes along with attendance.
        /// </summary>
        /// <param name="instructionDayDate">
        /// The date for which to get the details.
        /// </param>
        /// <returns>
        /// A static copy of the day's information as a DayInfo object.
        /// </returns>
        public DayInfo GetExactDayInfo(DateTimeOffset instructionDayDate)
        {
            int day = (int)instructionDayDate.DayOfWeek;
            DateTimeOffset date = new DateTimeOffset(instructionDayDate.Year, instructionDayDate.Month, instructionDayDate.Day, 0, 0, 0, new TimeSpan(5, 30, 0));
            DayInfo dayInfo = new DayInfo();

            foreach(ClassHours classHours in _weekRegularClasses[day])
            {
                AttendanceStub stub;
                classHours.Parent.Attendance.Details.TryGetValue(date, out stub);
                dayInfo.RegularClassDetails.Add(new KeyValuePair<ClassHours, AttendanceStub>(classHours, stub));
            }
            foreach(LtpCourse course in _weekNeglectedCourses[day])
            {
                AttendanceStub stub;
                if (course.Attendance.Details.TryGetValue(date, out stub))
                    dayInfo.ExtraClassDetails.Add(new KeyValuePair<LtpCourse, AttendanceStub>(course, stub));
            }

            return dayInfo;
        }

        #endregion

    }

    public class DayInfo
    {
        public List<KeyValuePair<ClassHours, AttendanceStub>> RegularClassDetails
        {
            set;
            get;
        }
        public List<KeyValuePair<LtpCourse, AttendanceStub>> ExtraClassDetails
        {
            set;
            get;
        }

        public DayInfo()
        {
            RegularClassDetails = new List<KeyValuePair<ClassHours, AttendanceStub>>();
            ExtraClassDetails = new List<KeyValuePair<LtpCourse, AttendanceStub>>();
        }

    }

}
