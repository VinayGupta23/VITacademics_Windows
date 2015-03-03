using Academics.DataModel;
using System;
using System.Globalization;
using Windows.Data.Json;


namespace Academics.ContentService
{
    /// <summary>
    /// Provides static methods to parse and return objects from Json strings.
    /// </summary>
    public static class JsonParser
    {
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
        public static User TryParseDataAsync(string jsonString)
        {
            try
            {
                User user;
                JsonObject rootObject = JsonObject.Parse(jsonString);

                string regNo = rootObject.GetNamedString("reg_no");
                DateTimeOffset dob = DateTimeOffset.ParseExact(rootObject.GetNamedString("dob"), "ddMMyyyy", CultureInfo.InvariantCulture);
                string campus = rootObject.GetNamedString("campus");
                user = new User(regNo, dob, campus);

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
                        case 5:
                            course = new PBCCourse();
                            break;
                        default:
                            continue;
                    }
                    AssignCourseDetails(course, courseObj);
                    user.AddCourse(course);
                }
                user.CoursesMetadata = new CoursesMetadata(
                            rootObject.GetNamedString("semester"),
                            DateTimeOffset.Parse(rootObject.GetNamedString("refreshed"), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind));
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
            ushort total = (ushort)attendanceObject.GetNamedNumber("total_classes");
            ushort attended = (ushort)attendanceObject.GetNamedNumber("attended_classes");
            double percentage = attendanceObject.GetNamedNumber("attendance_percentage");
            course.Attendance = new Attendance(course, total, attended, percentage);

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
                DateTimeOffset start = GetUtcTime(classHoursObject.GetNamedString("start_time"));
                DateTimeOffset end = GetUtcTime(classHoursObject.GetNamedString("end_time"));
                DayOfWeek day = (DayOfWeek)((int)classHoursObject.GetNamedNumber("day") + 1);
                course.AddClassHoursInstance(new ClassHours(course, start, end, day));
            }
        }
        private static MarksInfo GetMarksInfo(LtpCourse course, string marksType, JsonObject marksObject)
        {
            if (marksObject.GetNamedValue(marksType).ValueType == JsonValueType.Null)
            {
                return new MarksInfo(course, null, "");
            }
            else
            {
                return new MarksInfo(course,
                                     marksObject.GetNamedNumber(marksType),
                                     marksObject.GetNamedString(marksType + "_status"));
            }
        }
        private static DateTimeOffset GetUtcTime(string timeString)
        {
            return DateTimeOffset.Parse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
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
            course.Faculty = courseObject.GetNamedString("faculty");
            course.Title = courseObject.GetNamedString("course_title");
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
            nltpCourse.Credits = courseObject.GetNamedString("ltpc").Substring(3);
        }

        // Depth 2 Assignment (CBL, PBL, ...)
        private static void AssignSpecificDetails(CBLCourse course, JsonObject courseObject)
        {
            JsonObject marksObject = courseObject.GetNamedObject("marks");

            for (int i = 0; i < 3; i++)
                course._quizMarks[i] = GetMarksInfo(course, "quiz" + (i + 1), marksObject);
            for (int i = 0; i < 2; i++)
                course._catMarks[i] = GetMarksInfo(course, "cat" + (i + 1), marksObject);
            course.AssignmentMarks = GetMarksInfo(course, "assignment", marksObject);
        }
        private static void AssignSpecificDetails(LBCCourse course, JsonObject courseObject)
        {
            course.LabCamMarks = GetMarksInfo(course, "lab_cam", courseObject.GetNamedObject("marks"));
        }
        private static void AssignSpecificDetails(PBLCourse course, JsonObject courseObject)
        {
            JsonArray marksArray = courseObject.GetNamedObject("marks").GetNamedArray("details");

            foreach(JsonValue marksValue in marksArray)
            {
                JsonObject marksObject = marksValue.GetObject();

                string title = marksObject.GetNamedString("title");
                int maxMarks = (int)marksObject.GetNamedNumber("max_marks");
                int weightage = (int)marksObject.GetNamedNumber("weightage");

                PBLCourse.PBLMarkInfo markInfo;
                if(marksObject.GetNamedValue("conducted_on").ValueType == JsonValueType.Null)
                {
                    markInfo = new PBLCourse.PBLMarkInfo(course, title, maxMarks, weightage, null, null, "");
                }
                else
                {
                    markInfo = new PBLCourse.PBLMarkInfo(course, title, maxMarks, weightage,
                                                         DateTimeOffset.ParseExact(marksObject.GetNamedString("conducted_on"), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                                                         marksObject.GetNamedNumber("scored_mark"),
                                                         marksObject.GetNamedString("status"));
                }
                course._pblMarks.Add(markInfo);
            }
        }
        private static void AssignSpecificDetails(PBCCourse course, JsonObject courseObject)
        {
            course.ProjectTitle = courseObject.GetNamedString("project_title");
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
