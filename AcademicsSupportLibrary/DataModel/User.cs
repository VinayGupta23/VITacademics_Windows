using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    public class User
    {
        private Timetable _timetable;

        public string RegNo
        {
            get;
            private set;
        }
        public DateTimeOffset DateOfBirth
        {
            get;
            private set;
        }
        public string Campus
        {
            get;
            private set;
        }

        public List<Course> Courses { get; set; }
        public Timetable Timetable
        {
            get { return _timetable; }
        }

        public User(string regNo, DateTimeOffset dateOfBirth, string campus)
        {
            RegNo = regNo;
            DateOfBirth = dateOfBirth;
            Campus = campus;
        }

    }
}