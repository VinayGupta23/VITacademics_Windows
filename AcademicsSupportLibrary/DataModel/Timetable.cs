using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Academics.DataModel
{
    class Timetable
    {
        public IReadOnlyDictionary<ClassHours, Course> this[DayOfWeek day]
        {

        }
    }
}
