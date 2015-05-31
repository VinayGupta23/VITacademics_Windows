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
        public string ExamHeldOn { get; internal set; }

        internal string Id { get; set; }

        public void AssignExamDate(string yearMonthString)
        {
            Id = yearMonthString;
            DateTime date = DateTime.ParseExact(yearMonthString, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture);
            this.ExamHeldOn = date.ToString("MMMM, yyyy");
        }
    }

    public sealed class SemesterInfo : ReadOnlyCollection<GradeInfo>, IComparable<SemesterInfo>
    {
        public string CompletionMonth { get; internal set; }
        public ushort CreditsEarned { get; internal set; }
        public double Gpa { get; internal set; }

        internal string Id { get; set; }

        public SemesterInfo(IList<GradeInfo> grades)
            : base(grades)
        {
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
