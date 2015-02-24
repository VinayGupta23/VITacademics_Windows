using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    abstract class Course
    {
        public ushort ClassNumber { get; set; }
        public string CourseCode { get; set; }
        public string Title { get; set; }
        public string CourseMode { get; set; }
        public string CourseOption { get; set; }
        public string Ltpc { get; set; }
        public string Faculty { get; set; }
    }

    abstract class LtpCourse : Course
    {
        public string Slot { get; set; }
        public string Venue { get; set; }
        public List<ClassHours> CourseTimings { get; set; }
    }

    abstract class NonLtpCourse : Course
    {
        
    }

    public sealed class CBLCourse : LtpCourse
    {

    }

    public sealed class PBLCourse : LtpCourse
    {

    }

    public sealed class PBCCourse : NonLtpCourse
    {

    }

}
