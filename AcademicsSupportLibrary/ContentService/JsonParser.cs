using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.Data.Json;


namespace Academics.ContentService
{
    /// <summary>
    /// Provides static methods to parse and return objects from Json strings.
    /// </summary>
    public static class JsonParser
    {

        /* Note:
         * 
         * I. All times and dates are converted to IST when generating the data,
         *    since it is only then relevant, due to following reasons:
         * 1. Usage of the app from different locales must not display changed (different) class hours.
         *    On user request, timings can be changed, but it is the front end's responsibility.
         *    
         *    However (on the contrary), refresh date must be retained in its universal time format for consistency across regions in which the client may travel. 
         *
         * II. Any course that is not supported is skipped from the list as of the current parsing.
         * 
         */

        /// <summary>
        /// Returns the status shown on the Json string passed, or a suitable error code.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        internal static StatusCode GetStatus(string jsonString)
        {
            try
            {
                JsonObject statusObj = JsonObject.Parse(jsonString).GetNamedObject("status");
                int code = (int)statusObj.GetNamedNumber("code");
                StatusCode statusCode;

                switch (code)
                {
                    case 0:
                        statusCode = StatusCode.Success;
                        break;
                    case 11:
                        statusCode = StatusCode.SessionTimeout;
                        break;
                    case 12:
                        statusCode = StatusCode.InvalidCredentials;
                        break;
                    case 13:
                        statusCode = StatusCode.TemporaryError;
                        break;
                    case 89:
                    case 97:
                        statusCode = StatusCode.ServerError;
                        break;
                    case 98:
                        statusCode = StatusCode.UnderMaintenance;
                        break;
                    default:
                        statusCode = StatusCode.UnknownError;
                        break;
                }
                return statusCode;
            }
            catch
            {
                return StatusCode.InvalidData;
            }
        }

        /// <summary>
        /// Parses the Json string and returns a new User instance populated with all details. On failure, the method returns null.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static User TryParseData(string jsonString)
        {
            try
            {
                User user;
                JsonObject rootObject = JsonObject.Parse(jsonString);

                string regNo = rootObject.GetNamedString("reg_no");
                DateTimeOffset dob = DateTimeOffset.ParseExact(rootObject.GetNamedString("dob"), "ddMMyyyy", CultureInfo.InvariantCulture);
                string campus = rootObject.GetNamedString("campus");
                user = new User(regNo, dob, campus);

                ushort totalCredits = 0;
                JsonArray coursesArray = rootObject.GetNamedArray("courses");
                foreach (JsonValue courseValue in coursesArray)
                {
                    JsonObject courseObj = courseValue.GetObject();
                    Course course;
                    int courseType = (int)courseObj.GetNamedNumber("course_type");
                    switch (courseType)
                    {
                        case 1:
                            course = new CBLCourse();
                            break;
                        case 2:
                            course = new LBCCourse();
                            break;
                        case 3:
                            course = new PBLCourse();
                            break;
                        case 4:
                            course = new RBLCourse();
                            break;
                        case 5:
                        case 6:
                            course = new PBCCourse();
                            break;
                        default:
                            continue;
                    }
                    AssignCourseDetails(course, courseObj);
                    user.AddCourse(course);
                    totalCredits += course.Credits;
                }
                user.CoursesMetadata = new CoursesMetadata(
                            rootObject.GetNamedString("semester"),
                            DateTimeOffset.Parse(rootObject.GetNamedString("refreshed"), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind),
                            totalCredits);
                return user;
            }
            catch
            {
                return null;
            }
        }


        #region Helper Methods

        private static void AssignAttendance(LtpCourse course, JsonObject attendanceObject)
        {
            int classLength = 1;
            if(course as LBCCourse != null)
            {
                classLength = (int)Char.GetNumericValue(course.Ltpc[2]);
            }

            ushort total = (ushort)attendanceObject.GetNamedNumber("total_classes");
            ushort attended = (ushort)attendanceObject.GetNamedNumber("attended_classes");
            double percentage = attendanceObject.GetNamedNumber("attendance_percentage");
            course.Attendance = new Attendance(course, total, attended, percentage, classLength);

            JsonArray detailsArray = attendanceObject.GetNamedArray("details");
            foreach (JsonValue stubValue in detailsArray)
            {
                JsonObject stubObject = stubValue.GetObject();
                DateTime classDate = DateTime.ParseExact(stubObject.GetNamedString("date"), "yyyy-MM-dd", CultureInfo.InvariantCulture);

                course.Attendance.AddAttendanceStub(
                    new DateTimeOffset(classDate, new TimeSpan(5, 30, 0)),
                    stubObject.GetNamedString("status"),
                    stubObject.GetNamedString("reason"));
            }
        }
        private static void AssignTimings(LtpCourse course, JsonArray timingsArray)
        {
            foreach (JsonValue classHoursValue in timingsArray)
            {
                JsonObject classHoursObject = classHoursValue.GetObject();
                DateTimeOffset start = GetTime(classHoursObject.GetNamedString("start_time"));
                DateTimeOffset end = GetTime(classHoursObject.GetNamedString("end_time"));
                DayOfWeek day = (DayOfWeek)((int)classHoursObject.GetNamedNumber("day") + 1);
                course.AddClassHoursInstance(new ClassHours(course, start, end, day));
            }
        }
        private static MarksInfo GetMarksInfo(LtpCourse course, string markTitle, string marksType, JsonObject marksObject)
        {
            if (marksObject.GetNamedValue(marksType).ValueType == JsonValueType.Null)
            {
                return new MarksInfo(course, markTitle, null, "");
            }
            else
            {
                return new MarksInfo(course, markTitle,
                                     marksObject.GetNamedNumber(marksType),
                                     marksObject.GetNamedString(marksType + "_status"));
            }
        }
        private static void AssignCustomMarks(LtpCourse course, List<CustomMarkInfo> customMarks, JsonArray marksArray)
        {
            foreach (JsonValue marksValue in marksArray)
            {
                JsonObject marksObject = marksValue.GetObject();

                string title = marksObject.GetNamedString("title");
                int maxMarks = (int)marksObject.GetNamedNumber("max_marks");
                int weightage = (int)marksObject.GetNamedNumber("weightage");

                CustomMarkInfo markInfo;
                if (marksObject.GetNamedValue("conducted_on").ValueType == JsonValueType.Null)
                {
                    markInfo = new CustomMarkInfo(course, title, maxMarks, weightage, null, null, "");
                }
                else
                {
                    markInfo = new CustomMarkInfo(course, title, maxMarks, weightage,
                                                         new DateTimeOffset(DateTime.ParseExact(marksObject.GetNamedString("conducted_on"), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                                                            new TimeSpan(5, 30, 0)),
                                                         marksObject.GetNamedNumber("scored_mark"),
                                                         marksObject.GetNamedString("status"));
                    if (markInfo.Marks != null)
                    {
                        course.InternalMarksScored += Math.Round((double)markInfo.Marks * weightage / markInfo.MaxMarks, 2);
                        course.TotalMarksTested += markInfo.Weightage;
                    }
                }
                customMarks.Add(markInfo);
            }
        }
        private static DateTimeOffset GetTime(string timeString)
        {
            return new DateTimeOffset(
                (DateTime.Parse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)),
                new TimeSpan(5, 30, 0));
        }
        private static string RomanNumeral(int x)
        {
            if (x == 1)
                return "I";
            if (x == 2)
                return "II";
            if (x == 3)
                return "III";
            if (x == 4)
                return "IV";
            else
                throw new NotImplementedException();
        }

        #endregion

        #region Hierarchical Course Construction Methods

        // Depth 0 Assignment (Course)
        private static void AssignRootTypeDetails(Course course, JsonObject courseObject)
        {
            course.ClassNumber = (ushort)courseObject.GetNamedNumber("class_number");
            course.CourseCode = courseObject.GetNamedString("course_code");
            course.CourseMode = courseObject.GetNamedString("course_mode");
            course.CourseOption = courseObject.GetNamedString("course_option");
            course.SubjectType = courseObject.GetNamedString("subject_type");
            course.Faculty = courseObject.GetNamedString("faculty");
            course.Title = courseObject.GetNamedString("course_title");
            course.Credits = (ushort)int.Parse(courseObject.GetNamedString("ltpc").Substring(3));
        }

        // Depth 1 Assignment (Ltp and NonLtp)
        private static void AssignBaseTypeDetails(LtpCourse ltpCourse, JsonObject courseObject)
        {
            ltpCourse.Ltpc = courseObject.GetNamedString("ltpc");
            ltpCourse.Slot = courseObject.GetNamedString("slot");
            ltpCourse.Venue = courseObject.GetNamedString("venue");

            AssignTimings(ltpCourse, courseObject.GetNamedArray("timings"));
            AssignAttendance(ltpCourse, courseObject.GetNamedObject("attendance"));
        }
        private static void AssignBaseTypeDetails(NonLtpCourse nltpCourse, JsonObject courseObject)
        {
        }

        // Depth 2 Assignment (CBL, PBL, ...)
        private static void AssignSpecificDetails(CBLCourse course, JsonObject courseObject)
        {
            JsonObject marksObject = courseObject.GetNamedObject("marks");
            double scored = 0, total = 0;
            double? temp;

            // Quiz Marks
            for (int i = 0; i < 3; i++)
            {
                course._quizMarks[i] = GetMarksInfo(course, "Quiz " + RomanNumeral(i + 1), "quiz" + (i + 1), marksObject);
                temp = course._quizMarks[i].Marks;
                if (temp != null)
                {
                    scored += (double)temp;
                    total += 5;
                }
            }
            // CAT Marks
            for (int i = 0; i < 2; i++)
            {
                course._catMarks[i] = GetMarksInfo(course, "CAT " + RomanNumeral(i + 1), "cat" + (i + 1), marksObject);
                temp = course._catMarks[i].Marks;
                if (temp != null)
                {
                    scored += ((double)temp * 15 / 50);
                    total += 15;
                }
            }
            // Assignment Marks
            course.AssignmentMarks = GetMarksInfo(course, "Assignment", "assignment", marksObject);
            temp = course.AssignmentMarks.Marks;
            if (temp != null)
            {
                scored += (double)temp;
                total += 5;
            }

            course.InternalMarksScored = Math.Round(scored, 2);
            course.TotalMarksTested = total;
        }
        private static void AssignSpecificDetails(LBCCourse course, JsonObject courseObject)
        {
            course.Title += " Lab";
            course.LabCamMarks = GetMarksInfo(course,"Lab CAM", "lab_cam", courseObject.GetNamedObject("marks"));
            double? temp = course.LabCamMarks.Marks;
            if (temp != null)
            {
                course.InternalMarksScored = Math.Round((double)temp, 2);
                course.TotalMarksTested = 50;
            }
        }
        private static void AssignSpecificDetails(PBLCourse course, JsonObject courseObject)
        {
            JsonArray marksArray = courseObject.GetNamedObject("marks").GetNamedArray("details");
            AssignCustomMarks(course, course._pblMarks, marksArray);
        }
        private static void AssignSpecificDetails(RBLCourse course, JsonObject courseObject)
        {
            JsonArray marksArray = courseObject.GetNamedObject("marks").GetNamedArray("details");
            AssignCustomMarks(course, course._rblMarks, marksArray);
        }
        private static void AssignSpecificDetails(PBCCourse course, JsonObject courseObject)
        {
            if (courseObject.GetNamedValue("project_title").ValueType != JsonValueType.Null)
                course.ProjectTitle = courseObject.GetNamedString("project_title");
            else
                course.ProjectTitle = null;
        }

        // Private API
        private static void AssignCourseDetails(dynamic course, JsonObject courseObject)
        {
            AssignRootTypeDetails(course, courseObject);
            AssignBaseTypeDetails(course, courseObject);
            AssignSpecificDetails(course, courseObject);
        }

        #endregion

    }
}
