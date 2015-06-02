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
using System.Threading.Tasks;
using Academics.DataModel;
using System.ComponentModel;
using VITacademics.Helpers;
using Windows.UI.Popups;


namespace VITacademics.UIControls
{
    public sealed partial class GradesControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {

        public class CourseGradePair : INotifyPropertyChanged
        {
            private readonly string _courseTitle;
            private readonly ushort _credits;
            private char _grade;

            public char Grade
            {
                get { return _grade; }
                set
                {
                    _grade = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Grade"));
                }
            }
            public string CourseTitle
            {
                get { return _courseTitle; }
            }
            public ushort Credits
            {
                get { return _credits; }
            }

            public event PropertyChangedEventHandler PropertyChanged;

            public CourseGradePair(string courseTitle, ushort credits)
            {
                _courseTitle = courseTitle;
                _credits = credits;
                Grade = '-';
            }
        }

        private List<char> _grades = new List<char>(8) {'S', 'A', 'B', 'C', 'D', 'E', 'F', 'N' };
        private AcademicHistory _academicHistory;
        private List<CourseGradePair> _courses;
        private CourseGradePair tempSelection;

        public AcademicHistory GradeHistory
        {
            private set
            {
                _academicHistory = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("GradeHistory"));
                    PropertyChanged(this, new PropertyChangedEventArgs("RefreshDate"));
                }
                if (value != null)
                {
                    gradeGroups.Source = value.SemesterGroups;
                }
            }
            get { return _academicHistory; }
        }
        public List<CourseGradePair> CourseGradePairs
        {
            set
            {
                _courses = value;
                if (PropertyChanged != null)
                    PropertyChanged(this, new PropertyChangedEventArgs("CourseGradePairs"));
            }
            get { return _courses; }
        }
        public List<char> Grades
        {
            get { return _grades; }
        }

        public GradesControl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public event EventHandler<RequestEventArgs> ActionRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        public async void GenerateView(string parameter)
        {
            if (GradeHistory == null)
            {
                var response = await UserManager.GetGradesFromCacheAsync();
                if (response.Code == Academics.ContentService.StatusCode.Success)
                {
                    GradeHistory = response.Content;
                }
            }

            CourseGradePairs = new List<CourseGradePair>();
            var uniqueCourseGroups = UserManager.CurrentUser.Courses.GroupBy<Course, string>((Course c) => c.CourseCode);
            foreach (var courseGroup in uniqueCourseGroups)
            {
                ushort credits = 0;
                Course course = null;
                foreach (Course c in courseGroup)
                {
                    if (course == null)
                        course = c;
                    credits += c.Credits;
                }
                CourseGradePairs.Add(new CourseGradePair(course.Title, credits));
            }
        }

        public Dictionary<string, object> SaveState()
        {
            return null;
        }
        public void LoadState(Dictionary<string, object> lastState)
        {
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {

            if (UserManager.IsBusy)
            {
                await new MessageDialog("Please wait while your last requested task completes.", "Busy").ShowAsync();
                return;
            }
            refreshButton.IsEnabled = false;
            refreshButton.Content = "refreshing...";

            var response = await UserManager.RequestGradesFromServerAsync();
            if (response.Code == Academics.ContentService.StatusCode.Success)
            {
                GradeHistory = response.Content;
            }
            else
            {
                await StandardMessageDialogs.GetDialog(response.Code).ShowAsync();
            }

            refreshButton.IsEnabled = true;
            refreshButton.Content = "refresh";
        }

        private void SelectGradeButton_Click(object sender, RoutedEventArgs e)
        {
            gradeListPicker.ShowAt((FrameworkElement)sender);
            tempSelection = (sender as FrameworkElement).DataContext as CourseGradePair;
        }

        private void GradePicker_ItemsPicked(ListPickerFlyout sender, ItemsPickedEventArgs args)
        {
            if (tempSelection != null)
            {
                tempSelection.Grade = (char)gradeListPicker.SelectedItem;
            }
            tempSelection = null;
            gradeListPicker.SelectedIndex = -1;
        }

    }
}
