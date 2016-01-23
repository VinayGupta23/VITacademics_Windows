using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using VITacademics.Managers;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using VITacademics.Helpers;
using System.ComponentModel;
using Academics.DataModel;

namespace VITacademics.UIControls
{
    public sealed partial class ReminderControl : UserControl
    {
        public Dictionary<string, TimeSpan> ReminderPresets
        { get; private set; }
        public DateTimeOffset ContextDate
        { get; set; }
        public Course ContextCourse
        { get; set; }
        public TimeSpan StartTime
        { get; set; }
        public TimeSpan Duration
        { get; set; }
        public TimeSpan Reminder
        { get; set; }
        public string Agenda
        { get; set; }

        public ReminderControl()
        {
            this.InitializeComponent();
            DataContextChanged += ReminderControl_DataContextChanged;

            ReminderPresets = new Dictionary<string, TimeSpan>();
            ReminderPresets.Add("at start time", TimeSpan.Zero);
            ReminderPresets.Add("15 minutes", TimeSpan.FromMinutes(15));
            ReminderPresets.Add("30 minutes", TimeSpan.FromMinutes(30));
            ReminderPresets.Add("1 hour", TimeSpan.FromHours(1));
            ReminderPresets.Add("4 hours", TimeSpan.FromHours(4));
            ReminderPresets.Add("12 hours", TimeSpan.FromHours(12));
            ReminderPresets.Add("1 day", TimeSpan.FromDays(1));
            reminderComboBox.ItemsSource = ReminderPresets.Keys;
        }

        private void ReminderControl_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            if (DataContext != null)
            {
                CalendarAwareStub stub = DataContext as CalendarAwareStub;
                StartTime = stub.StartTime;
                Duration = stub.Duration;
                ContextDate = stub.ContextDate;
                ContextCourse = stub.ContextCourse;

                if (stub.ApptInfo != null)
                {
                    ToggleVisiblityForAgendaInput();
                    Agenda = stub.ApptInfo.Subject;
                    string reminderKey = null;
                    foreach (var pair in ReminderPresets)
                    {
                        if (pair.Value == stub.ApptInfo.Reminder)
                            reminderKey = pair.Key;
                    }
                    if (reminderKey != null)
                        reminderComboBox.SelectedItem = reminderKey;
                    else
                        reminderComboBox.SelectedIndex = 0;
                }
                else
                {
                    reminderComboBox.SelectedIndex = 1;
                    agendaComboBox.ItemsSource = GetAgendaList(stub.ContextCourse);
                    Agenda = "";
                }
                
                DataContextChanged -= ReminderControl_DataContextChanged;
                this.DataContext = this;
            }
        }

        private void ToggleVisiblityForAgendaInput()
        {
            agendaComboBox.Visibility = Visibility.Collapsed;
            agendaTextBox.Visibility = Visibility.Visible;
        }

        private void AgendaComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (agendaComboBox.SelectedIndex < 0)
                return;

            if (agendaComboBox.SelectedValue as string == "type...")
            {
                ToggleVisiblityForAgendaInput();
                agendaTextBox.Focus(FocusState.Programmatic);
            }
            else
                Agenda = agendaComboBox.SelectedValue as string;
        }

        private void ReminderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Reminder = ReminderPresets[(string)reminderComboBox.SelectedItem];
        }

        private void AgendaTextBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                datePicker.Focus(FocusState.Programmatic);
        }

        private List<string> GetAgendaList(Course course)
        {
            List<string> agendaList = null;

            switch (course.CourseMode)
            {
                case "CBL":
                    agendaList = new List<string>() { "Quiz I", "Quiz II", "Quiz III", "Class Test", "Assignment Deadline" };
                    break;
                case "LBC":
                    agendaList = new List<string>() { "VIVA Test", "Quiz", "Record Submission", "Mid-Term Examination", "Term-end Examination" };
                    break;
                case "PBL":
                case "RBL":
                    agendaList = new List<string>() { "Class Test", "Quiz", "Mid-Term Examination", "Project/Report Submission" };
                    break;
                default:
                    agendaList = new List<string>() { "Class Test", "Internal Assessment", "Mid-Term Examination" };
                    break;
            }

            if (course is LtpCourse)
            {
                agendaList.Add("Extra Class");
                agendaList.Add("Class Cancelled");
            }
            agendaList.Add("type...");
            return agendaList;
        }


    }
}
