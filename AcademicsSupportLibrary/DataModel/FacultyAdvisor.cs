using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    public class FacultyAdvisor
    {
        public string Name { get; internal set; }
        public string Designation { get; internal set; }
        public string School { get; internal set; }
        public string Division { get; internal set; }
        public string Phone { get; internal set; }
        public string Email { get; internal set; }
        public string Cabin { get; internal set; }
        public string Intercom { get; internal set; }

        internal FacultyAdvisor()
        { }
    }
}
