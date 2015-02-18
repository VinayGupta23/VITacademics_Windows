using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Academics.DataModel;


namespace Academics.ContentService
{
    /// <summary>
    /// Provides static methods to parse and return objects from Json strings.
    /// </summary>
    public static class JsonParser
    {
        /// <summary>
        /// Returns the status shown on the Json string passed, or a suitable error code.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        internal static StatusCode GetStatus(string jsonString)
        {
            try
            {
                JsonObject statusObj = JsonObject.Parse(jsonString).GetNamedObject("status");
                int code = (int)statusObj.GetNamedNumber("code");
                StatusCode statusCode;

                switch (code)
                {
                    case 0:
                        statusCode = StatusCode.Success;
                        break;
                    case 11:
                        statusCode = StatusCode.SessionTimeout;
                        break;
                    case 12:
                        statusCode = StatusCode.InvalidCredentials;
                        break;
                    case 13:
                        statusCode = StatusCode.TemporaryError;
                        break;
                    case 89:
                    case 97:
                        statusCode = StatusCode.ServerError;
                        break;
                    case 98:
                        statusCode = StatusCode.UnderMaintenance;
                        break;
                    default:
                        statusCode = StatusCode.UnknownError;
                        break;
                }
                return statusCode;
            }
            catch
            {
                return StatusCode.InvalidData;
            }
        }

        /// <summary>
        /// Parses the Json string and assigns all details to the passed User instance. Returns true on success.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        /// <remarks>
        /// It is recommended that a (deep) copy of the original user instance be passed, to avoid partial data assignment on potential failure.
        /// </remarks>
        public static Task<bool> TryParseDataAsync(User user, string jsonString)
        {
            return
                Task.Run(() => 
                {
                    return false;
                }
                );
        }
    }
}
