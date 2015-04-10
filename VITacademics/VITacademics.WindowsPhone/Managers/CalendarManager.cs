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

        public static async Task WriteAppointmentAsync(CalenderAwareInfoStub infoStub, string message)
        {
            if (_calendar == null)
                throw new InvalidOperationException();

            Appointment appt;
            if (infoStub.AppointmentInfo == null)
            {
                appt = new Appointment();
                appt.StartTime = infoStub.ContextDate.Date.Add(infoStub.SessionHours.StartHours.TimeOfDay);
                appt.Duration = infoStub.SessionHours.EndHours - infoStub.SessionHours.StartHours;
                appt.Reminder = TimeSpan.FromMinutes(15);
                appt.Location = infoStub.SessionHours.Parent.Venue;
            }
            else
            {
                appt = await _calendar.GetAppointmentAsync(infoStub.AppointmentInfo.Item1);
            }

            appt.Subject = string.Format("{0} - {1}", infoStub.SessionHours.Parent.CourseCode, message);
            await _calendar.SaveAppointmentAsync(appt);
            infoStub.AppointmentInfo = new Tuple<string, string>(appt.LocalId, appt.Subject);
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Calendar Access", "Set reminder", null, 0);
#endif
        }

        public static async Task RemoveAppointmentAsync(CalenderAwareInfoStub infoStub)
        {
            if (_calendar == null)
                throw new InvalidOperationException();

            if (infoStub.AppointmentInfo == null)
                return;

            await _calendar.DeleteAppointmentAsync(infoStub.AppointmentInfo.Item1);
            infoStub.AppointmentInfo = null;
        }

        public static async Task AssignAppointmentIfAvailableAsync(CalenderAwareInfoStub infoStub)
        {
            if (_calendar == null)
                throw new InvalidOperationException();

            DateTimeOffset startDate = infoStub.ContextDate.Date.Add(infoStub.SessionHours.StartHours.TimeOfDay);
            var appts = await _calendar.FindAppointmentsAsync(startDate, TimeSpan.FromMinutes(1));
            if (appts.Count == 0)
                infoStub.AppointmentInfo = null;
            else
                infoStub.AppointmentInfo = new Tuple<string, string>(appts[0].LocalId, appts[0].Subject);
        }

        #endregion
    }
}
