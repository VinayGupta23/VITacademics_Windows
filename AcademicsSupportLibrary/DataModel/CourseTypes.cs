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
        internal List<CustomMarkInfo> _pblMarks;

        public ReadOnlyCollection<CustomMarkInfo> PblMarks { get; private set; }

        public PBLCourse()
        {
            _pblMarks = new List<CustomMarkInfo>();
            PblMarks = new ReadOnlyCollection<CustomMarkInfo>(_pblMarks);
        }
    }

    public sealed class RBLCourse : LtpCourse
    {
        internal List<CustomMarkInfo> _rblMarks;

        public ReadOnlyCollection<CustomMarkInfo> RblMarks { get; private set; }

        public RBLCourse()
        {
            _rblMarks = new List<CustomMarkInfo>();
            RblMarks = new ReadOnlyCollection<CustomMarkInfo>(_rblMarks);
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
