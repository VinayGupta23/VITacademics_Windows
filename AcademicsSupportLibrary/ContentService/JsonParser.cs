using Academics.DataModel;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
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
        /// Returns a bare instance of the user whose content the json string contains. If the json string is invalid, returns null.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static User GetJsonStringOwner(string jsonString)
        {
            try
            {
                JsonObject rootObject = JsonObject.Parse(jsonString);
                string regNo = rootObject.GetNamedString("reg_no");
                DateTimeOffset dob = DateTimeOffset.ParseExact(rootObject.GetNamedString("dob"), "ddMMyyyy", CultureInfo.InvariantCulture);
                string campus = rootObject.GetNamedString("campus");
                string phoneNo = null;
                if (campus == "chennai")
                    phoneNo = "NA";
                else
                    phoneNo = rootObject.GetNamedString("mobile");
                return new User(regNo, dob, campus, phoneNo);
            }
            catch
            {
                return null;
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
                User user = GetJsonStringOwner(jsonString);
                JsonObject rootObject = JsonObject.Parse(jsonString);
                
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
                            rootObject.GetNamedString("semester"), GetRefreshUTC(rootObject.GetNamedString("refreshed")), totalCredits);
                return user;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Parses the Json string and returns the complete academic history of the user. On failure, the method returns null.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        public static AcademicHistory TryParseGrades(string jsonString)
        {
            try
            {
                AcademicHistory academicHistory = null;
                JsonObject rootObject = JsonObject.Parse(jsonString);

                // Adding complete list of raw grades
                JsonArray gradesArray = rootObject.GetNamedArray("grades");
                academicHistory = new AcademicHistory(gradesArray.Count);
                foreach (JsonValue gradeValue in gradesArray)
                    academicHistory._grades.Add(GetGradeInfo(gradeValue));

                // Adding semester-wise grades and gpa
                var groupedGrades = academicHistory.Grades.GroupBy<GradeInfo, string>((GradeInfo gradeInfo) => { return gradeInfo.Id; });                                                       
                JsonArray semesterWiseArray = rootObject.GetNamedArray("semester_wise");
                var semInfoList = groupedGrades.Join<IGrouping<string, GradeInfo>, IJsonValue, string, SemesterInfo>(
                                            semesterWiseArray,
                                            (group) => { return group.Key; },
                                            (semValue) => { return semValue.GetObject().GetNamedString("exam_held"); },
                                            (IGrouping<string, GradeInfo> group, IJsonValue value) =>
                                            {
                                                JsonObject semesterInfoObject = value.GetObject();
                                                SemesterInfo semesterInfo = new SemesterInfo(group.ToList<GradeInfo>());
                                                semesterInfo.CreditsEarned = (ushort)semesterInfoObject.GetNamedNumber("credits");
                                                semesterInfo.Gpa = semesterInfoObject.GetNamedNumber("gpa");
                                                return semesterInfo;
                                            });
                foreach(var semInfo in semInfoList)
                    academicHistory._semesterGroupedGrades.Add(semInfo);
                academicHistory._semesterGroupedGrades.Sort();

                // Adding summary data
                academicHistory.Cgpa = rootObject.GetNamedNumber("cgpa");
                academicHistory.CreditsRegistered = (ushort)rootObject.GetNamedNumber("credits_registered");
                academicHistory.CreditsEarned = (ushort)rootObject.GetNamedNumber("credits_earned");
                academicHistory.LastRefreshed = GetRefreshUTC(rootObject.GetNamedString("grades_refreshed"));

                return academicHistory;
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
            if (course as LBCCourse != null)
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
                DateTimeOffset classDate = new DateTimeOffset(DateTime.ParseExact(stubObject.GetNamedString("date"), "yyyy-MM-dd", CultureInfo.InvariantCulture), new TimeSpan(5, 30, 0));

                course.Attendance.AddStubToDetails(classDate, new AttendanceStub(
                                                                    stubObject.GetNamedString("slot"),
                                                                    stubObject.GetNamedString("status"),
                                                                    stubObject.GetNamedString("reason")));
            }
        }
        private static void AssignTimings(LtpCourse course, JsonArray timingsArray)
        {
            foreach (JsonValue classHoursValue in timingsArray)
            {
                JsonObject classHoursObject = classHoursValue.GetObject();
                DateTimeOffset start = GetIST(classHoursObject.GetNamedString("start_time"));
                DateTimeOffset end = GetIST(classHoursObject.GetNamedString("end_time"));
                DayOfWeek day = (DayOfWeek)((int)classHoursObject.GetNamedNumber("day") + 1);
                course.AddClassHoursInstance(new ClassHours(course, start, end, day));
            }
        }
        private static void AssignMarks(LtpCourse course, JsonObject marksObject)
        {
            double marksScored = 0;
            JsonArray marksArray = marksObject.GetNamedArray("assessments");
            foreach (JsonValue marksValue in marksArray)
            {
                JsonObject markStubObject = marksValue.GetObject();
                string title = markStubObject.GetNamedString("title");
                int maxMarks = (int)markStubObject.GetNamedNumber("max_marks");
                int weightage = (int)markStubObject.GetNamedNumber("weightage");

                MarkInfo markInfo;
                if (markStubObject.GetNamedValue("scored_marks").ValueType == JsonValueType.Null)
                    markInfo = new MarkInfo(course, title, maxMarks, weightage, null, null, "");
                else
                {
                    markInfo = new MarkInfo(course, title, maxMarks, weightage,
                                    null, // currently, 'conducted date' is being skipped.
                                    markStubObject.GetNamedNumber("scored_marks"),
                                    markStubObject.GetNamedString("status"));

                    marksScored += (double)markInfo.WeightedMarks;
                    course.TotalMarksTested += markInfo.Weightage;
                }
                course.AddMarkInfo(markInfo);
            }
            
            course.InternalMarksScored = Math.Round(marksScored, 2);
        }

        private static GradeInfo GetGradeInfo(JsonValue gradeValue)
        {
            JsonObject gradeObject = gradeValue.GetObject();
            GradeInfo info = new GradeInfo();
            info.CourseCode = gradeObject.GetNamedString("course_code");
            info.CourseTitle = gradeObject.GetNamedString("course_title");
            info.CourseType = gradeObject.GetNamedString("course_type");
            info.CourseOption = gradeObject.GetNamedString("option").ToUpper();
            info.Credits = (ushort)gradeObject.GetNamedNumber("credits");
            info.Grade = gradeObject.GetNamedString("grade")[0];
            info.AssignExamDate(gradeObject.GetNamedString("exam_held"));

            if (info.CourseOption == "NIL")
                info.CourseOption = "";

            return info;
        }

        private static DateTimeOffset GetIST(string timeString)
        {
            return new DateTimeOffset(
                (DateTime.Parse(timeString, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal)),
                new TimeSpan(5, 30, 0));
        }
        private static DateTimeOffset GetRefreshUTC(string refreshDateString)
        {
            return DateTimeOffset.Parse(refreshDateString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
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

            JsonObject marksObject = courseObject.GetNamedObject("marks");
            AssignMarks(ltpCourse, marksObject);

        }
        private static void AssignBaseTypeDetails(NonLtpCourse nltpCourse, JsonObject courseObject)
        {
        }

        // Depth 2 Assignment (CBL, PBL, ...)
        private static void AssignSpecificDetails(CBLCourse course, JsonObject courseObject)
        {
        }
        private static void AssignSpecificDetails(LBCCourse course, JsonObject courseObject)
        {
            course.Title += " Lab";
        }
        private static void AssignSpecificDetails(PBLCourse course, JsonObject courseObject)
        {
        }
        private static void AssignSpecificDetails(RBLCourse course, JsonObject courseObject)
        {
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
