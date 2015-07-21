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

        private static int FindInsertionIndex(CalendarAwareDayInfo dayInfo, TimeSpan startTime)
        {
            int i = 0;
            for (i = 0; i < dayInfo.RegularClassesInfo.Count; i++)
            {
                if (TimeSpan.Compare(startTime, (dayInfo.RegularClassesInfo[i].StartTime)) <= 0)
                    break;
            }
            return i;
        }

        private static async Task WriteAppointmentCoreAsync(Appointment appt, string subjectKey, string subject, DateTimeOffset reminderDate, TimeSpan startTime, TimeSpan duration, TimeSpan reminder)
        {
            appt.Subject = GetAgendaFromComponents(subjectKey, subject);
            appt.StartTime = reminderDate.Date.Add(startTime);
            appt.Duration = duration;
            appt.Reminder = reminder;
            await _calendar.SaveAppointmentAsync(appt);
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Calendar Access", "Set reminder", null, 0);
#endif
        }

        private static string GetAgendaFromComponents(string key, string value)
        {
            return string.Format("{0} - {1}", key, value);
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

            foreach (var stub in dayInfo.RegularClassesInfo)
            {
                Appointment appt;
                try
                {
                    if (appts.Count == 0)
                        break;
                    appt = appts.First((Appointment a) => a.StartTime.TimeOfDay == stub.StartTime
                                    && string.Equals(a.Subject.Substring(0, 6), stub.ContextCourse.CourseCode) == true);
                }
                catch { appt = null; }

                if (appt != null)
                {
                    var fullAppt = await _calendar.GetAppointmentAsync(appt.LocalId);
                    stub.ApptInfo = new AppointmentInfo(fullAppt);
                    appts.Remove(appt);
                }
            }

            foreach (Appointment appt in appts)
            {
                var fullAppt = await _calendar.GetAppointmentAsync(appt.LocalId);
                int index = FindInsertionIndex(dayInfo, appt.StartTime.TimeOfDay);
                dayInfo.RegularClassesInfo.Insert(index, new CustomInfoStub(dayInfo.ContextDate, fullAppt));
            }
        }

        public static KeyValuePair<string, string> ExtractComponentsFromSubject(string subject)
        {
            int index = subject.IndexOf('-');
            return new KeyValuePair<string, string>(subject.Substring(0, index).Trim(), subject.Substring(index + 1).Trim());
        }

        #endregion
    }
}
