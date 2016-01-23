using Academics.ContentService;
using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Appointments;
using VITacademics.Helpers;


namespace VITacademics.Managers
{
    public static class CalendarManager
    {
        private const string CALENDAR_OWNER_KEY = "calendarOwner";

        private static AppointmentStore _store;
        private static AppointmentCalendar _calendar;

        private static string CalendarOwner
        {
            get
            {
                try { return App._localSettings.Values[CALENDAR_OWNER_KEY] as string; }
                catch { return null; }
            }
            set
            {
                App._localSettings.Values[CALENDAR_OWNER_KEY] = value;
            }
        }

        #region Private Helpers

        private static async Task EnsureAvailabilityAsync()
        {
            if (_store == null)
                _store = await AppointmentManager.RequestStoreAsync(AppointmentStoreAccessType.AppCalendarsReadWrite);
        }

        private static async Task WriteAppointmentCoreAsync(Appointment appt, string subjectKey, string subject, DateTimeOffset reminderDate, TimeSpan startTime, TimeSpan duration, TimeSpan reminder)
        {
            appt.Subject = string.Format("{0} - {1}", subjectKey, subject);
            appt.StartTime = reminderDate.Date.Add(startTime);
            appt.Duration = duration;
            appt.Reminder = reminder;
            await _calendar.SaveAppointmentAsync(appt);
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Calendar Access", "Set reminder", null, 0);
#endif
        }

        #endregion

        #region Management Specific API

        internal static async Task DeleteCalendarAsync()
        {
            await EnsureAvailabilityAsync();
            var appCalendars = await _store.FindAppointmentCalendarsAsync();
            foreach (AppointmentCalendar calendar in appCalendars)
                await calendar.DeleteAsync();
            _calendar = null;
            CalendarOwner = null;
        }

        internal static async Task CreateNewCalendarAsync(User requester)
        {
            await DeleteCalendarAsync();
            
            AppointmentCalendar calendar = await _store.CreateAppointmentCalendarAsync("Academics Calendar");
            calendar.OtherAppReadAccess = AppointmentCalendarOtherAppReadAccess.SystemOnly;
            calendar.OtherAppWriteAccess = AppointmentCalendarOtherAppWriteAccess.None;
            await calendar.SaveAsync();

            _calendar = calendar;
            CalendarOwner = requester.RegNo;
        }

        #endregion

        #region Public Methods

        public static async Task LoadCalendarAsync()
        {
            if (UserManager.CurrentUser == null)
                throw new InvalidOperationException("There is no calendar to get.");

            if (CalendarOwner != UserManager.CurrentUser.RegNo)
                await CreateNewCalendarAsync(UserManager.CurrentUser);

            if (_calendar != null)
                return;

            await EnsureAvailabilityAsync();
            _calendar = (await _store.FindAppointmentCalendarsAsync())[0];
        }

        public static async Task ModifyAppointmentAsync(string reminderLocalId, string subject, DateTimeOffset reminderDate, TimeSpan startTime, TimeSpan duration, TimeSpan reminder)
        {
            if (_calendar == null)
                throw new InvalidOperationException();

            if (reminderLocalId == null)
                throw new ArgumentNullException("reminderLocalId");

            Appointment appt = await _calendar.GetAppointmentAsync(reminderLocalId);
            string code = CalendarManager.ExtractComponentsFromSubject(appt.Subject).Key;
            await WriteAppointmentCoreAsync(appt, code, subject, reminderDate, startTime, duration, reminder);
        }

        public static async Task WriteAppointmentAsync(Course contextCourse, string subject, DateTimeOffset reminderDate, TimeSpan startTime, TimeSpan duration, TimeSpan reminder)
        {
            if (_calendar == null)
                throw new InvalidOperationException();

            Appointment appt = new Appointment();
            LtpCourse ltpCourse = contextCourse as LtpCourse;
            if (ltpCourse != null)
                appt.Location = ltpCourse.Venue;

            string code = contextCourse.CourseCode;
            if (contextCourse is LBCCourse)
                code = code + " Lab";

            await WriteAppointmentCoreAsync(appt, code, subject, reminderDate, startTime, duration, reminder);
        }

        public static async Task RemoveAppointmentAsync(string localId)
        {
            if (_calendar == null)
                throw new InvalidOperationException();

            await _calendar.DeleteAppointmentAsync(localId);
        }

        public static async Task LoadRemindersAsync(CalendarAwareDayInfo dayInfo)
        {
            if (_calendar == null)
                throw new InvalidOperationException();
            List<Appointment> appts = (await _calendar.FindAppointmentsAsync(dayInfo.ContextDate.Date, TimeSpan.FromDays(1))).ToList();
            if (appts.Count == 0)
                return;

            if (dayInfo.IsEmptyDay)
            {
                foreach (Appointment a in appts)
                {
                    Appointment fullAppt = await _calendar.GetAppointmentAsync(a.LocalId);
                    dayInfo.RegularClassesInfo.Add(new CustomInfoStub(dayInfo.ContextDate, fullAppt));
                }
                return;
            }

            int i, j;
            Appointment appt = null;
            TimeSpan apptStart = default(TimeSpan);
            for (i = 0, j = 0; j < appts.Count; )
            {
                if (appt == null)
                {
                    appt = await _calendar.GetAppointmentAsync(appts[j].LocalId);
                    apptStart = appt.StartTime.TimeOfDay;
                }
                CalendarAwareStub stub = dayInfo.RegularClassesInfo[i];
                TimeSpan classStart = stub.StartTime;

                if (apptStart == classStart && GetContextCourse(appt).ClassNumber == stub.ContextCourse.ClassNumber
                        && stub.ApptInfo == null)
                    stub.ApptInfo = new AppointmentInfo(appt);
                else if (apptStart <= classStart)
                    dayInfo.RegularClassesInfo.Insert(i++, new CustomInfoStub(dayInfo.ContextDate, appt));
                else
                {
                    if (i < dayInfo.RegularClassesInfo.Count - 1)
                    {
                        i++;
                        continue;
                    }
                    else
                        dayInfo.RegularClassesInfo.Add(new CustomInfoStub(dayInfo.ContextDate, appt));
                }
                j++;
                appt = null;
            }
        }

        public static Course GetContextCourse(Appointment appt)
        {
            if (appt == null)
                return null;

            string key = CalendarManager.ExtractComponentsFromSubject(appt.Subject).Key;
            string[] parts = key.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string courseCode = parts[0].Trim();

            if (parts.Length > 1)
                return UserManager.CurrentUser.Courses.First((Course c) => c.CourseCode == courseCode && c.CourseMode == "LBC");
            else
                return UserManager.CurrentUser.Courses.First((Course c) => c.CourseCode == courseCode && c.CourseMode != "LBC");
        }

        public static KeyValuePair<string, string> ExtractComponentsFromSubject(string subject)
        {
            int index = subject.IndexOf('-');
            return new KeyValuePair<string, string>(subject.Substring(0, index).Trim(), subject.Substring(index + 1).Trim());
        }

        #endregion
    }
}
