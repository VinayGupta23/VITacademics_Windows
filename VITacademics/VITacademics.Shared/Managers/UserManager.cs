using System;
using System.Text;
using Academics.DataModel;
using Academics.ContentService;
using Windows.Security.Credentials;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Storage;


namespace VITacademics.Managers
{
    /// <summary>
    /// Maintains the current user, and provides functionality to request and load user data, store credentials and cache content.
    /// </summary>
    public static class UserManager
    {
        #region Constants and Fields

        private const string RESOURCE_NAME = "VITacademics";
        private const string JSON_FILE_NAME = "UserData.txt";
        private const string CACHE_DATA_OWNER_KEY = "cachedData_owner";
        private const string CACHE_DATA_TIME_KEY = "cachedData_time";

        private static StorageFolder _folder = ApplicationData.Current.RoamingFolder;

        #endregion

        #region Properties

        private static string CachedDataOwner
        {
            get
            {
                try
                {
                    return App._roamingSettings.Values[CACHE_DATA_OWNER_KEY] as string;
                }
                catch
                {
                    return null;
                }
            }
            set
            {
                App._roamingSettings.Values[CACHE_DATA_OWNER_KEY] = value;
            }
        }
        public static User CurrentUser
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets the last point of time the user data was changed (saved or deleted), in UTC format. If unavailable, gets the default value of the data type.
        /// </summary>
        public static DateTimeOffset CachedDataLastChanged
        {
            get
            {
                try
                {
                    return (DateTimeOffset)App._roamingSettings.Values[CACHE_DATA_TIME_KEY];
                }
                catch
                {
                    return default(DateTimeOffset);
                }
            }
            private set
            {
                App._roamingSettings.Values[CACHE_DATA_TIME_KEY] = value;
            }
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Checks if credentials are saved in the Credential Locker and returns the credentials if found, otherwise returns null.
        /// </summary>
        /// <returns></returns>
        private static PasswordCredential GetStoredCredential()
        {
            try
            {
                var storedCredentials = new PasswordVault().RetrieveAll();
                if (storedCredentials.Count == 0)
                    return null;
                else
                    return storedCredentials[0];
            }
            catch
            {
                return null;
            }
        }

        private static async Task<bool> TryCacheDataAsync(string jsonString)
        {
            bool res = false;
            try
            {
                CachedDataOwner = null;
                StorageFile dataFile = await _folder.CreateFileAsync(JSON_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                res = await StorageHelper.TryWriteAsync(dataFile, jsonString);
                if (res == true)
                    CachedDataOwner = CurrentUser.RegNo;
            }
            finally
            {
                CachedDataLastChanged = DateTimeOffset.UtcNow;
            }
            return res;
        }

        /// <summary>
        /// Parses the Json string to assigns details and returns a fresh instance of the populated user. On failure, the method returns null.
        /// </summary>
        /// <param name="jsonString"></param>
        /// <returns></returns>
        private static async Task<User> ParseDataAsync(string jsonString)
        {
            User tempUser = new User(CurrentUser.RegNo, CurrentUser.DateOfBirth, CurrentUser.Campus);
            bool result = await JsonParser.TryParseDataAsync(tempUser, jsonString);
            if (result == true)
                return tempUser;
            else
                return null;
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Checks the Credential Locker and assigns current user if available. 
        /// </summary>
        static UserManager()
        {
            PasswordCredential credential = GetStoredCredential();
            if (credential != null)
            {
                try
                {
                    credential = new PasswordVault().Retrieve(RESOURCE_NAME, credential.UserName);
                    // Parse "password" to retrieve DOB and campus
                    DateTimeOffset dob = DateTimeOffset.ParseExact(credential.Password.Substring(0, 8), "ddMMyyyy", CultureInfo.InvariantCulture);
                    string campus = credential.Password.Substring(8);
                    CurrentUser = new User(credential.UserName, dob, campus);
                }
                catch
                {
                    // Corrupt data
                    DeleteSavedUser();
                }
            }
            else
                CurrentUser = null;
        }

        #endregion

        #region Public Methods (API)

        /// <summary>
        /// Assigns the current user (and logs in) and saves the credentials to the Locker, if a login with the passed parameters was successful.
        /// </summary>
        /// <remarks>
        /// Note: Any existing credentials in the Locker are overwritten on success.
        /// </remarks>
        /// <returns>
        ///  Returns a specific status code as per the login attempt result.
        /// </returns>
        public static async Task<StatusCode> CreateNewUserAsync(string regNo, DateTimeOffset dateOfBirth, string campus)
        {
            User user = new User(regNo, dateOfBirth, campus);
            StatusCode status = await NetworkService.TryLoginAsync(user);

            if (status == StatusCode.Success)
            {
                DeleteSavedUser();
                try
                {
                    // Store Credentials in the following format: "VITacademics" - "{regNo}" : "{ddMMyyyy}{Campus}"
                    new PasswordVault().Add(
                        new PasswordCredential(RESOURCE_NAME, regNo, dateOfBirth.ToString("ddMMyyyy", CultureInfo.InvariantCulture) + campus));
                }
                catch { }
                CurrentUser = user;
            }
            return status;
        }

        /// <summary>
        /// Attempts a login with the current user.
        /// </summary>
        /// <remarks>
        /// This method returns a status code of NoData if the current user is null.
        /// </remarks>
        /// <returns>
        /// Returns the specific status code as per the response.
        /// </returns>
        public static async Task<StatusCode> LoginUserAsync()
        {
            if (CurrentUser != null)
            {
                StatusCode code = await NetworkService.TryLoginAsync(CurrentUser);
                return code;
            }
            else
            {
                return StatusCode.InvalidRequest;
            }
        }

        /// <summary>
        /// Clears any saved credentials in the Locker and sets the current user to null.
        /// </summary>
        public static void DeleteSavedUser()
        {
            CurrentUser = null;
            try
            {
                PasswordCredential credential = GetStoredCredential();
                new PasswordVault().Remove(credential);
            }
            catch { }
        }

        /// <summary>
        /// Refreshes and assigns the user details by requesting fresh data from the server. On success, the data is also cached before the function returns.
        /// </summary>
        /// <returns>
        /// Returns a status code containing information about the request's result.
        /// </returns>
        public static async Task<StatusCode> RefreshFromServerAsync()
        {
            if (CurrentUser == null)
                return StatusCode.InvalidRequest;

            try
            {
                Response<string> response = await NetworkService.TryGetDataAsync(CurrentUser);
                if (response.Code != StatusCode.Success)
                    return response.Code;

                User temp = await ParseDataAsync(response.Content);
                if (temp == null)
                    return StatusCode.UnknownError;

                CurrentUser = temp;
                await TryCacheDataAsync(response.Content);
                return StatusCode.Success;
            }
            catch
            {
                return StatusCode.UnknownError;
            }
        }

        /// <summary>
        /// Assigns the user details by reading data from the cache.
        /// </summary>
        /// <returns>
        /// Status code indicating result of operation.
        /// </returns>
        public static async Task<StatusCode> LoadCacheAsync()
        {
            try
            {
                if (CurrentUser == null)
                    return StatusCode.InvalidRequest;

                if (CachedDataOwner != CurrentUser.RegNo)
                    return StatusCode.NoData;

                StorageFile file = await _folder.GetFileAsync(JSON_FILE_NAME);
                string jsonString = await StorageHelper.TryReadAsync(file);
                User temp = await ParseDataAsync(jsonString);
                if (temp == null)
                    return StatusCode.UnknownError;

                CurrentUser = temp;
                return StatusCode.Success;
            }
            catch
            {
                return StatusCode.NoData;
            }
        }

        #endregion

    }
}
