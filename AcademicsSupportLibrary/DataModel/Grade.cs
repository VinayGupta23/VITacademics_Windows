using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    public class GradeInfo
    {
        public string CourseCode { get; internal set; }
        public string CourseTitle { get; internal set; }
        public string CourseType { get; internal set; }
        public string CourseOption { get; internal set; }
        public ushort Credits { get; internal set; }
        public char Grade { get; internal set; }
        public DateTime ExamMonth { get; internal set; }

        internal string Id { get; set; }

        public void AssignExamDate(string yearMonthString)
        {
            Id = yearMonthString;
            ExamMonth = DateTime.ParseExact(yearMonthString, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
        }

    }

    public sealed class SemesterInfo : ReadOnlyCollection<GradeInfo>, IComparable<SemesterInfo>
    {
        public string Title { get; internal set; }
        public DateTime CompletionMonth { get; internal set; }
        public ushort CreditsEarned { get; internal set; }
        public double Gpa { get; internal set; }
        internal string Id { get; set; }

        public SemesterInfo(IList<GradeInfo> grades)
            : base(grades)
        {
            if (this.Count != 0)
            {
                this.AssignTitle();
                this.CompletionMonth = this[0].ExamMonth;
                this.Id = this[0].Id;
            }
        }

        private void AssignTitle()
        {
            DateTime dateId = this[0].ExamMonth;
            string sem;
            int year1, year2;

            switch (dateId.Month)
            {
                case 11:
                    sem = "Fall";
                    break;
                case 5:
                    sem = "Winter";
                    break;
                case 7:
                    sem = "Summer";
                    break;
                default:
                    sem = "Semester";
                    break;
            }

            if (dateId.Month > 7)
            {
                year1 = dateId.Year;
                year2 = year1 + 1;
            }
            else
            {
                year2 = dateId.Year;
                year1 = year2 - 1;
            }

            this.Title = String.Format("{0} {1}-{2}", sem, year1.ToString(), (year2 % 100).ToString());
        }

        public int CompareTo(SemesterInfo other)
        {
            return string.Compare(this.Id, other.Id);
        }
    }

    public sealed class AcademicHistory
    {
        internal List<GradeInfo> _grades;
        internal List<SemesterInfo> _semesterGroupedGrades;

        public ReadOnlyCollection<GradeInfo> Grades { get; private set; }
        public ReadOnlyCollection<SemesterInfo> SemesterGroups { get; private set; }

        public ushort CreditsRegistered { get; internal set; }
        public ushort CreditsEarned { get; internal set; }
        public double Cgpa { get; internal set; }
        public DateTimeOffset LastRefreshed { get; internal set; }
        public DateTimeOffset LastRefreshedLocal { get { return LastRefreshed.ToLocalTime(); } }

        public AcademicHistory(int size)
        {
            _grades = new List<GradeInfo>(size);
            _semesterGroupedGrades = new List<SemesterInfo>();

            Grades = new ReadOnlyCollection<GradeInfo>(_grades);
            SemesterGroups = new ReadOnlyCollection<SemesterInfo>(_semesterGroupedGrades);
        }

    }

}
