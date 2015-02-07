using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;
using Academics.DataModel;


namespace Academics.ContentService
{
    public enum StatusCode
    {
        Success = 0,
        InvalidCredentials = 1,
        NoInternet = 2
    }

    static class NetworkService
    {
        private static const string BASE_URI_STRING = "http://vitacademics-dev.herokuapp.com";
        private static const string LOGIN_STRING = "/api/v2/{0}/login?regno={1}&dob={2}";
        private static const string REGISTER_STRING = "/api/v2/{0}/register?regno={1}&dob={2}";
        private static const string REFRESH_STRING = "/api/v2/{0}/refresh?regno={1}&dob={2}";

        public async Task<StatusCode> AttemptLogin(User user)
        {
            
        }
    }
}
