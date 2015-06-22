using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace Academics.DataModel
{
    /// <summary>
    /// A timetable view of courses which provides easy access to day wise information such as class schedule and attendance.
    /// </summary>
    public class Timetable
    {

        #region Private Fields and Helper Plug-ins

        private readonly List<ClassHours>[] _weekRegularClasses = new List<ClassHours>[7];
        private readonly List<LtpCourse>[] _weekNeglectedCourses = new List<LtpCourse>[7];

        private static int ClassHoursComparision(ClassHours x, ClassHours y)
        {
            return DateTimeOffset.Compare(x.StartHours, y.StartHours);
        }

        private static AttendanceGroup GetAttendanceGroup(AttendanceDetails details, DateTimeOffset date)
        {
            if (details.Contains(date))
                return details[date];
            else
                return null;
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
            DateTimeOffset date = new DateTimeOffset(instructionDayDate.Date, new TimeSpan(5, 30, 0));
            DayInfo dayInfo = new DayInfo();
            Dictionary<Course, int> courseBlotCount = new Dictionary<Course, int>();

            foreach (ClassHours classHours in _weekRegularClasses[day])
            {
                if (courseBlotCount.ContainsKey(classHours.Parent))
                    courseBlotCount[classHours.Parent] += 1;
                else
                    courseBlotCount[classHours.Parent] = 0;

                AttendanceGroup group = GetAttendanceGroup(classHours.Parent.Attendance._details, date);
                AttendanceStub stub = null;
                if (group != null)
                    if (group.Details.Count > courseBlotCount[classHours.Parent])
                        stub = group.Details[courseBlotCount[classHours.Parent]];
                dayInfo.RegularClassDetails.Add(new KeyValuePair<ClassHours, AttendanceStub>(classHours, stub));
            }
            foreach (LtpCourse course in _weekNeglectedCourses[day])
            {
                AttendanceGroup group = GetAttendanceGroup(course.Attendance._details, date);
                if (group != null)
                {
                    foreach (AttendanceStub stub in group.Details)
                        dayInfo.ExtraClassDetails.Add(new KeyValuePair<LtpCourse, AttendanceStub>(course, stub));
                }
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
