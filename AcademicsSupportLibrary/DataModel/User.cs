using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    public class User
    {
        private List<Course> _courses;

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
        public ReadOnlyCollection<Course> Courses
        {
            get; private set;
        }
        public CoursesMetadata CoursesMetadata
        {
            get;
            internal set;
        }

        public User(string regNo, DateTimeOffset dateOfBirth, string campus)
        {
            RegNo = regNo;
            DateOfBirth = dateOfBirth;
            Campus = campus;

            _courses = new List<Course>();
            Courses = new ReadOnlyCollection<Course>(_courses);
        }

        internal void AddCourse(Course course)
        {
            _courses.Add(course);
        }

    }
}