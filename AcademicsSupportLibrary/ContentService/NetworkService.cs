using System;
using System.Threading.Tasks;
using Windows.Web.Http;
using Academics.DataModel;
using Windows.Web.Http.Filters;


namespace Academics.ContentService
{
    /// <summary>
    /// Provides static methods to get response content along with status by sending HTTP/server requests.
    /// </summary>
    public static class NetworkService
    {
        public enum Options
        {
            RefreshCourses,
            CompleteData
        }

        private const string BASE_URI_STRING = "http://vitacademics-dev.herokuapp.com";
        private const string LOGIN_STRING_FORMAT = "/api/v2/{0}/login?regno={1}&dob={2}";
        private const string REGISTER_STRING_FORMAT = "/api/v2/{0}/register?regno={1}&dob={2}";
        private const string REFRESH_STRING_FORMAT = "/api/v2/{0}/refresh?regno={1}&dob={2}";
        // To be changed
        private const string WP_USER_AGENT = "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)";
        private const int MAX_ATTEMPTS = 2;

        private static readonly HttpClient _httpClient;

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
                string uriString = BASE_URI_STRING + String.Format(relativeUriFormat, user.Campus, user.RegNo, user.DateOfBirth.ToString("ddMMyyyy", System.Globalization.CultureInfo.InvariantCulture));
                HttpResponseMessage httpResponse = await _httpClient.GetAsync(new Uri(uriString));

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

        static NetworkService()
        {
            // Prevent caching of data locally to avoid errors and ensure fresh data every request.
            HttpBaseProtocolFilter filter = new HttpBaseProtocolFilter();
            filter.CacheControl.ReadBehavior = Windows.Web.Http.Filters.HttpCacheReadBehavior.MostRecent;
            filter.CacheControl.WriteBehavior = Windows.Web.Http.Filters.HttpCacheWriteBehavior.NoCache;

            _httpClient = new HttpClient(filter);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(WP_USER_AGENT);
        }

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
        /// Get the data as a Json string along with status code for the specified user and request option.
        /// </summary>
        /// <param name="user">
        /// The user whose data to request.
        /// </param>
        /// <param name="getDataOption">
        /// The type of data to request.
        /// </param>
        /// <remarks>
        /// Note: This method attempts a login and a single retry upon receiving a SessionTimedOut error.
        /// </remarks>
        /// <returns>
        /// A response containing status code and content. Returns the Json string as the content on success, otherwise the content is null.
        /// </returns>
        public static async Task<Response<string>> TryGetDataAsync(User user, Options getDataOption)
        {
            string relativeUriFormat;
            if (getDataOption == Options.CompleteData)
                relativeUriFormat = REGISTER_STRING_FORMAT;
            else
                relativeUriFormat = REFRESH_STRING_FORMAT;

            Response<string> response = await GetResponse(relativeUriFormat, user);

            if (response.Code == StatusCode.SessionTimeout)
            {
                StatusCode loginStatus = await TryLoginAsync(user);
                if (loginStatus == StatusCode.Success)
                    response = await GetResponse(relativeUriFormat, user);
                else
                    response = new Response<string>(loginStatus, null);
            }

            return response;
        }
    }
}
