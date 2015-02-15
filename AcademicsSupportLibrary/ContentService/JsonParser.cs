using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;


namespace Academics.ContentService
{
    /// <summary>
    /// Provides static methods to parse and return objects from Json strings.
    /// </summary>
    public static class JsonParser
    {
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
                    case 98:
                        statusCode = StatusCode.ServerError;
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
    }
}
