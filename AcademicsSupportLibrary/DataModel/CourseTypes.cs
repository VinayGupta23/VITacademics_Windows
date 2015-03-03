using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Academics.DataModel
{

    public sealed class CBLCourse : LtpCourse
    {
        internal MarksInfo[] _quizMarks;
        internal MarksInfo[] _catMarks;

        public ReadOnlyCollection<MarksInfo> QuizMarks { get; private set; }
        public ReadOnlyCollection<MarksInfo> CatMarks { get; private set; }
        public MarksInfo AssignmentMarks { get; internal set; }

        public CBLCourse()
        {
            _quizMarks = new MarksInfo[3];
            _catMarks = new MarksInfo[2];
            QuizMarks = new ReadOnlyCollection<MarksInfo>(_quizMarks);
            CatMarks = new ReadOnlyCollection<MarksInfo>(_catMarks);
        }
    }

    public sealed class PBLCourse : LtpCourse
    {
        public class PBLMarkInfo : LtpCourseComponent
        {
            private readonly string _marksTitle;
            private readonly int _maxMarks;
            private readonly int _weightage;
            private readonly DateTimeOffset? _conductedDate;
            private readonly double? _marks;
            private readonly string _status;

            public string MarksTitle
            {
                get
                { return _marksTitle; }
            }
            public int MaxMarks
            {
                get
                { return _maxMarks; }
            }
            public int Weightage
            {
                get
                { return _weightage; }
            }
            public DateTimeOffset? ConductedDate
            {
                get
                { return _conductedDate; }
            }
            public double? Marks
            {
                get
                { return _marks; }
            }
            public string Status
            {
                get
                { return _status; }
            }

            public PBLMarkInfo(LtpCourse parent, string marksTitle, int maxMarks, int weightage, DateTimeOffset? conductedDate, double? marks, string status)
                : base(parent)
            {
                _marksTitle = marksTitle;
                _maxMarks = maxMarks;
                _weightage = weightage;
                _conductedDate = conductedDate;
                _marks = marks;
                _status = status;
            }

        }

        internal List<PBLMarkInfo> _pblMarks;

        public ReadOnlyCollection<PBLMarkInfo> PblMarks { get; private set; }

        public PBLCourse()
        {
            _pblMarks = new List<PBLMarkInfo>();
            PblMarks = new ReadOnlyCollection<PBLMarkInfo>(_pblMarks);
        }
    }

    public sealed class LBCCourse : LtpCourse
    {
        public MarksInfo LabCamMarks { get; internal set; }
    }

    public sealed class PBCCourse : NonLtpCourse
    {
        public string ProjectTitle { get; internal set; }
    }

}
