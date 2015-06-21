using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace Academics.DataModel
{

    public sealed class CBLCourse : LtpCourse
    {
    }

    public sealed class PBLCourse : LtpCourse
    {
    }

    public sealed class RBLCourse : LtpCourse
    {
    }

    public sealed class LBCCourse : LtpCourse
    {
    }

    public sealed class PBCCourse : NonLtpCourse
    {
        public string ProjectTitle { get; internal set; }
    }

}
