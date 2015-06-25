using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using VITacademics.Helpers;
using VITacademics.Managers;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


namespace VITacademics.UIControls
{
    public sealed partial class GradesControl : UserControl, IProxiedControl, INotifyPropertyChanged
    {
        #region Dependency Nested Class

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

        #endregion

        #region Fields and Properties

        // Constants
        private readonly List<char> _grades = new List<char>(8) {'S', 'A', 'B', 'C', 'D', 'E', 'F', 'N' };
        public List<char> Grades
        {
            get { return _grades; }
        }

        // Fields
        private AcademicHistory _academicHistory;
        private List<CourseGradePair> _courseGradePairs;

        private CourseGradePair tempSelection;
        private ushort[] _predictedCredits;
        private string _predictedGpa = "-";
        private string _predictedCgpa = "-";

        // Properties
        public AcademicHistory GradeHistory
        {
            private set
            {
                _academicHistory = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged("RefreshDate");
                if (value != null)
                    gradeGroups.Source = value.SemesterGroups;
            }
            get { return _academicHistory; }
        }
        public List<CourseGradePair> CourseGradePairs
        {
            set
            {
                _courseGradePairs = value;
                NotifyPropertyChanged();
            }
            get { return _courseGradePairs; }
        }
        public ushort[] PredictedCredits
        {
            get { return _predictedCredits; }
            private set
            {
                _predictedCredits = value;
                NotifyPropertyChanged();
            }
        }
        public string PredictedGpa
        {
            get { return _predictedGpa; }
            set
            {
                if (_predictedGpa == value)
                    return;
                _predictedGpa = value;
                NotifyPropertyChanged();

            }
        }
        public string PredictedCgpa
        {
            get { return _predictedCgpa; }
            set
            {
                if (_predictedCgpa == value)
                    return;
                _predictedCgpa = value;
                NotifyPropertyChanged();
            }
        }

        #endregion

        public GradesControl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        #region Event Definitions and Related Handlers

        public event EventHandler<RequestEventArgs> ActionRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public string DisplayTitle
        {
            get { return "Grades"; }
        }

        public Dictionary<string, object> SaveState()
        {
            var grades = new List<char>();
            foreach (CourseGradePair cgPair in CourseGradePairs)
                grades.Add(cgPair.Grade);
            return new Dictionary<string, object>(1) { { "predictionGrades", grades } };
        }

        public async void LoadView(string parameter, Dictionary<string, object> lastState = null)
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
                if (string.Equals(courseGroup.ElementAt<Course>(0).CourseOption, "Audit", StringComparison.OrdinalIgnoreCase))
                    continue;

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

            try
            {
                if (lastState == null)
                    return;
                var grades = lastState["predictionGrades"] as List<char>;
                for (int i = 0; i < grades.Count; i++)
                    CourseGradePairs[i].Grade = grades[i];
            }
            catch { return; }
        }

        #region Private Helper Methods and Event Handlers

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

        private async void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (GradeHistory == null)
            {
                await new MessageDialog("Please refresh grades from the 'academic history' tab first to predict your GPA.", "No Data").ShowAsync();
                return;
            }

            if(CourseGradePairs.Count == 0)
            {
                await new MessageDialog("You have no courses this semester to predict the GPA.", "Oops...").ShowAsync();
                return;
            }

            double weighedSum = 0;
            ushort normalisingCredits = 0;
            ushort creditsEarnedNow = 0;

            foreach (CourseGradePair cgPair in CourseGradePairs)
            {
                if (!char.IsLetter(cgPair.Grade))
                {
                    await new MessageDialog("Please enter grade predictions for all courses to calculate the GPA.", "Missing Inputs").ShowAsync();
                    return;
                }

                if (cgPair.Grade == 'N')
                    continue;
                normalisingCredits += cgPair.Credits;
                if (cgPair.Grade == 'F')
                    continue;
                weighedSum += cgPair.Credits * GetGradePoint(cgPair.Grade);
                creditsEarnedNow += cgPair.Credits;
            }

            ushort creditsRegisteredNow = UserManager.CurrentUser.CoursesMetadata.TotalCredits;
            ushort creditsEarned = GradeHistory.CreditsEarned;
            ushort creditsRegistered = GradeHistory.CreditsRegistered;
            double gpa = weighedSum / normalisingCredits;
            double cgpa = (weighedSum + GradeHistory.Cgpa * creditsRegistered) / (creditsRegistered + creditsRegisteredNow);

            PredictedGpa = gpa.ToString("F2");
            PredictedCgpa = cgpa.ToString("F2");
            PredictedCredits = new ushort[4] { creditsEarnedNow,
                                               creditsRegisteredNow,
                                               (ushort)(creditsEarned + creditsEarnedNow),
                                               (ushort)(creditsRegistered + creditsRegisteredNow) };
        }

        private ushort GetGradePoint(char grade)
        {
            switch(grade)
            {
                case 'S': return 10;
                case 'A': return 9;
                case 'B': return 8;
                case 'C': return 7;
                case 'D': return 6;
                case 'E': return 5;
                default: return 0;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            PredictedCgpa = "-";
            PredictedGpa = "-";
            PredictedCredits = null;
            foreach (var cgPair in CourseGradePairs)
                cgPair.Grade = '-';
        }

        #endregion
    }
}
