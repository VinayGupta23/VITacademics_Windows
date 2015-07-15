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

        private static int FindInsertionIndex(CalendarAwareDayInfo dayInfo, DateTimeOffset startTime)
        {
            int i = 0;
            for (i = 0; i < dayInfo.RegularClassesInfo.Count; i++)
            {
                if (DateTimeOffset.Compare(startTime, (dayInfo.RegularClassesInfo[i].StartTime)) <= 0)
                    break;
            }
            return i;
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
        
        // !!!
        public static async Task WriteAppointmentAsync(CalendarAwareStub stub, string message, TimeSpan reminderSpan)
        {
            if (_calendar == null)
                throw new InvalidOperationException();
            var infoStub = stub as RegularInfoStub;

            Appointment appt;
            if (infoStub.ApptInfo == null)
            {
                appt = new Appointment();

                appt.StartTime = infoStub.ContextDate.Date.Add(infoStub.SessionHours.StartHours.TimeOfDay);
                appt.Duration = infoStub.SessionHours.EndHours - infoStub.SessionHours.StartHours;
                appt.Location = infoStub.SessionHours.Parent.Venue;
            }
            else
            {
                appt = await _calendar.GetAppointmentAsync(infoStub.ApptInfo.LocalId);
            }

            appt.Subject = string.Format("{0} - {1}", infoStub.SessionHours.Parent.CourseCode, message);
            appt.Reminder = reminderSpan;
            await _calendar.SaveAppointmentAsync(appt);
            infoStub.ApptInfo = new AppointmentInfo(appt);
#if !DEBUG
            GoogleAnalytics.EasyTracker.GetTracker().SendEvent("Calendar Access", "Set reminder", null, 0);
#endif
        }

        public static async Task RemoveAppointmentAsync(CalendarAwareStub infoStub)
        {
            if (_calendar == null)
                throw new InvalidOperationException();

            if (infoStub.ApptInfo == null)
                return;

            await _calendar.DeleteAppointmentAsync(infoStub.ApptInfo.LocalId);
            infoStub.ApptInfo = null;
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
                    appt = appts.First((Appointment a) => a.StartTime.TimeOfDay == stub.StartTime.TimeOfDay
                                    && string.Equals(a.Subject.Substring(0, 6), stub.ContextCourse.CourseCode) == true);
                }
                catch { appt = null; }

                if (appt != null)
                {
                    stub.ApptInfo = new AppointmentInfo(appt);
                    appts.Remove(appt);
                }
            }

            foreach (Appointment appt in appts)
            {
                int index = FindInsertionIndex(dayInfo, appt.StartTime);
                dayInfo.RegularClassesInfo.Insert(index, new CustomInfoStub(dayInfo.ContextDate, appt));
            }
        }

        #endregion
    }
}
