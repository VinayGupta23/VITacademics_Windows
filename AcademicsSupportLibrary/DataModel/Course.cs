using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    public abstract class Course
    {
        public ushort ClassNumber { get; set; }
        public string CourseCode { get; set; }
        public string Title { get; set; }
        public string CourseMode { get; set; }
        public string CourseOption { get; set; }
        public string Faculty { get; set; }
    }

    public abstract class LtpCourse : Course
    {
        public string Slot { get; set; }
        public string Venue { get; set; }
        public string Ltpc { get; set; }
        public IReadOnlyList<ClassHours> CourseTimings { get; set; }
    }

    public abstract class NonLtpCourse : Course
    {
        public string Credits { get; set; }
    }

    public sealed class CBLCourse : LtpCourse
    {
        public MarksInfo[] QuizMarks { get; set; }
        public MarksInfo[] CatMarks { get; set; }
        public MarksInfo AssignmentMarks { get; set; }

        public CBLCourse()
        {
            QuizMarks = new MarksInfo[3];
            CatMarks = new MarksInfo[2];
        }
    }

    public sealed class PBLCourse : LtpCourse
    {
        public class PBLMarks
        {
            public int MaxMarks { get; set; }
            public int Weightage { get; set; }
            public DateTimeOffset ConductedDate { get; set; }
            public MarksInfo Info { get; set; }
        }
    }

    public sealed class LBCCourse : LtpCourse
    {
        public MarksInfo LabCamMarks { get; set; }
    }

    public sealed class PBCCourse : NonLtpCourse
    {
        public string ProjectTitle { get; set; }
    }

}
