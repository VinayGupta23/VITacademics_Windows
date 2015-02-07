using System;
using System.Collections.Generic;
using System.Text;
using Academics.DataModel;
using Academics.ContentService;

namespace VITacademics
{
    static class UserManager
    {
        public static User CurrentUser
        {
            get;
            private set;
        }
    }
}
