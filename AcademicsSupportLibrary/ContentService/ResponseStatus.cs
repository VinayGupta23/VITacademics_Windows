using System;


namespace Academics.ContentService
{
    /// <summary>
    /// Status codes relating to network or parsing errors for the ContentService namespace.
    /// </summary>
    public enum StatusCode
    {
        /// <summary>
        /// Successful execution.
        /// </summary>
        Success = 0,
        /// <summary>
        /// Connectivity or network issues.
        /// </summary>
        NoInternet = 1,
        /// <summary>
        /// Invalid credentials were provided.
        /// </summary>
        InvalidCredentials = 2,
        /// <summary>
        /// Temporary errors such as parsing fails.
        /// </summary>
        TemporaryError = 3,
        /// <summary>
        /// Server errors such as "Unavailable", "Database Error" or "Gateway Timeout".
        /// </summary>
        ServerError = 4,
        /// <summary>
        /// The (running) session timed out.
        /// </summary>
        SessionTimeout = 5,
        /// <summary>
        /// The data was either in an invalid format or corrupted.
        /// </summary>
        InvalidData = 6,
        /// <summary>
        /// The requested resource does not exist or is unavailable.
        /// </summary>
        NoData = 7,
        /// <summary>
        /// An unknown error occured.
        /// </summary>
        UnknownError = 8
    }

    /// <summary>
    /// Stores the status and content of the response received for some request.
    /// </summary>
    /// <remarks>
    /// This class is intended to be used as a return type for methods that process a request. Any instance of this class is read-only.
    /// </remarks>
    /// <typeparam name="T">
    /// The type of content the response contains.
    /// </typeparam>
    public sealed class Response<T>
    {
        private readonly StatusCode _code;
        private readonly T _content;

        public StatusCode Code
        {
            get { return _code; }
        }
        public T Content
        {
            get { return _content; }
        }

        public Response(StatusCode code, T content)
        {
            _code = code;
            _content = content;
        }
    }
}
