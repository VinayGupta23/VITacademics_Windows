using Academics.DataModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Web.Http;
using Windows.Web.Http.Filters;


namespace Academics.ContentService
{
    /// <summary>
    /// Provides static methods to get raw response content along with status by sending HTTP/server requests.
    /// </summary>
    public static class NetworkService
    {
        #region Private fields and constants
        
#if DEBUG
        private const string BASE_URI_STRING = "https://vitacademics-staging.herokuapp.com";
#else
        private const string BASE_URI_STRING = "https://vitacademics-rel.herokuapp.com";
#endif
        private const string LOGIN_STRING_FORMAT = "/api/v2/{0}/login/";
        private const string REFRESH_STRING_FORMAT = "/api/v2/{0}/refresh/";
        private const string GRADES_STRING_FORMAT = "/api/v2/{0}/grades/";
        private const string ADVISOR_STRING_FORMAT = "/api/v2/{0}/advisor/";

#if WINDOWS_PHONE_APP
        private const string WP_USER_AGENT = "Mozilla/5.0 (Mobile; Windows Phone 8.1; Android 4.0; ARM; Trident/7.0; Touch; rv:11.0; IEMobile/11.0; NOKIA; Lumia 520) like iPhone OS 7_0_3 Mac OS X AppleWebKit/537 (KHTML, like Gecko) Mobile Safari/537";
#else
        private const string WP_USER_AGENT = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
#endif
        private const int MAX_ATTEMPTS = 2;
        private static readonly HttpClient _httpClient;
        
        #endregion

        #region Private Helper Methods and Constructor

        /// <summary>
        /// Returns the content and status of the specified network request. Content is null if request fails.
        /// </summary>
        /// <param name="relativeUriFormat">
        /// The (relative) uri format string into which user parameters will be introduced.
        /// </param>
        /// <param name="user"></param>
        /// <returns></returns>
        private static async Task<Response<string>> GetResponse(string relativeUriFormat, User user)
        {
            StatusCode statusCode = StatusCode.UnknownError;
            string content = null;
            try
            {
                string dob = user.DateOfBirth.ToString("ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture);
                var postContent = new HttpFormUrlEncodedContent(
                                    new KeyValuePair<string, string>[3] {
                                        new KeyValuePair<string, string>("regno", user.RegNo),
                                        new KeyValuePair<string, string>("dob", dob),
                                        new KeyValuePair<string, string>("mobile", user.PhoneNo)
                                    });

                string uriString = BASE_URI_STRING + String.Format(relativeUriFormat, user.Campus);
                HttpResponseMessage httpResponse = await _httpClient.PostAsync(new Uri(uriString), postContent);

                switch (httpResponse.StatusCode)
                {
                    case HttpStatusCode.Ok:
                        {
                            content = await httpResponse.Content.ReadAsStringAsync();
                            statusCode = JsonParser.GetStatus(content);
                            break;
                        }
                    case HttpStatusCode.GatewayTimeout:
                    case HttpStatusCode.ServiceUnavailable:
                        {
                            statusCode = StatusCode.ServerError;
                            break;
                        }
                    default:
                        {
                            statusCode = StatusCode.UnknownError;
                            break;
                        }
                }
            }
            catch
            {
                statusCode = StatusCode.NoInternet;
            }

            if (statusCode != StatusCode.Success)
                content = null;
            return new Response<string>(statusCode, content);
        }

        private static async Task<Response<string>> GetContentAsync(string relUriFormat, User user)
        {
            Response<string> response = await GetResponse(relUriFormat, user);

            if (response.Code == StatusCode.SessionTimeout)
            {
                StatusCode loginStatus = await TryLoginAsync(user);
                if (loginStatus == StatusCode.Success)
                    response = await GetResponse(relUriFormat, user);
                else
                    response = new Response<string>(loginStatus, null);
            }

            return response;
        }

        static NetworkService()
        {
            // Prevent caching of data locally to avoid errors and ensure fresh data on every request.
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
            filter.CacheControl.WriteBehavior = Windows.Web.Http.Filters.HttpCacheWriteBehavior.NoCache;

            _httpClient = new HttpClient(filter);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(WP_USER_AGENT);
        }

        #endregion

        #region Public Methods (API)

        /// <summary>
        /// Attempts to login the passed user and returns the status of the operation.
        /// </summary>
        /// <remarks>
        /// Note: On encountering an internal error, another attempt is made to login before the method returns.
        /// </remarks>
        /// <param name="user">
        /// The user to login.
        /// </param>
        /// <returns>
        /// Status code indicating result of the login attempt.
        /// </returns>
        public static async Task<StatusCode> TryLoginAsync(User user)
        {
            StatusCode statusCode = StatusCode.UnknownError;
            int i = 1;
            while (i++ <= MAX_ATTEMPTS)
            {
                statusCode = (await GetResponse(LOGIN_STRING_FORMAT, user)).Code;

                if (statusCode == StatusCode.TemporaryError         // If Error parsing the captcha (or)
                    || statusCode == StatusCode.InvalidCredentials) // If the captcha was parsed incorrectly
                    continue;                                       // Then attempt to login again
                else
                    break;
            }
            return statusCode;
        }

        /// <summary>
        /// Get the data as a Json string along with status code for the specified user by sending a Http request.
        /// </summary>
        /// <param name="user">
        /// The user whose data to request.
        /// </param>
        /// <remarks>
        /// Note: This method attempts a login and a single retry upon receiving a SessionTimedOut error.
        /// </remarks>
        /// <returns>
        /// A response containing status code and content. Returns the Json string as the content on success, otherwise the content is null.
        /// </returns>
        public static async Task<Response<string>> TryGetDataAsync(User user)
        {
            return await GetContentAsync(REFRESH_STRING_FORMAT, user);
        }

        ///<summary>
        /// Get the academic history as a Json string along with status code for the specified user by sending a Http request.
        /// </summary>
        /// <param name="user">
        /// The user whose data to request.
        /// </param>
        /// <remarks>
        /// Note: This method attempts a login and a single retry upon receiving a SessionTimedOut error.
        /// </remarks>
        /// <returns>
        /// A response containing status code and content. Returns the Json string as the content on success, otherwise the content is null.
        /// </returns>
        public static async Task<Response<string>> TryGetGradesAsync(User user)
        {
            return await GetContentAsync(GRADES_STRING_FORMAT, user);
        }

        public static async Task<Response<string>> TryGetAdvisorDetailsAsync(User user)
        {
            return await GetContentAsync(ADVISOR_STRING_FORMAT, user);
        }

        #endregion
    }
}
