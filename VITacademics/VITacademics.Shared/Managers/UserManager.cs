using System;
using System.Text;
using Academics.DataModel;
using Academics.ContentService;
using Windows.Security.Credentials;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Storage;
using System.ComponentModel;
using System.Runtime.CompilerServices;


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

        private static User _currentUser;
        private static StorageFolder _folder = ApplicationData.Current.RoamingFolder;
        private static bool _isBusy;
        private delegate Task<StatusCode> Function();

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
        public static User CurrentUser
        {
            get { return _currentUser; }
            private set
            {
                _currentUser = value;
                NotifyPropertyChanged();
            }
        }
        public static bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;
                NotifyPropertyChanged();
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

        private static async Task<StatusCode> MonitoredTask(Function func)
        {
            if (IsBusy == true)
                return StatusCode.InvalidRequest;

            IsBusy = true;
            StatusCode result = await func();
            IsBusy = false;
            return result;
        }

        #endregion

        #region Event Notifiers

        public static event PropertyChangedEventHandler PropertyChanged;

        private static void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
                PropertyChanged(null, new PropertyChangedEventArgs(propertyName));
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
#if WINDOWS_PHONE_APP
                    DeleteSavedUserAsync();
#else
                    DeleteSavedUser();
#endif
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
            return await MonitoredTask(async () =>
            {
                User user = new User(regNo, dateOfBirth, campus);
                StatusCode status = await NetworkService.TryLoginAsync(user);

                if (status == StatusCode.Success)
                {
                    try
                    {
                        PasswordCredential credential = GetStoredCredential();
                        if (credential != null)
                            new PasswordVault().Remove(credential);

                        // Store Credentials in the following format: "VITacademics" - "{regNo}" : "{ddMMyyyy}{Campus}"
                        new PasswordVault().Add(
                            new PasswordCredential(RESOURCE_NAME, regNo, dateOfBirth.ToString("ddMMyyyy", CultureInfo.InvariantCulture) + campus));

                        CurrentUser = user;
                    }
                    catch
                    {
                        status = StatusCode.UnknownError;
                    }
                }
                return status;
            }
            );
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
            return await MonitoredTask(async () =>
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
            });
        }

       
#if WINDOWS_PHONE_APP
        /// <summary>
        /// Clears any saved credentials in the Locker and sets the current user to null. This call also deletes the app calendar.
        /// </summary>
        public static async Task<StatusCode> DeleteSavedUserAsync()
        {
            return await MonitoredTask(async () =>
            {
                CurrentUser = null;
                await VITacademics.Helpers.CalendarHelper.DeleteCalendar();
                try
                {
                    PasswordCredential credential = GetStoredCredential();
                    new PasswordVault().Remove(credential);
                }
                catch { }
                return StatusCode.Success;
            });
        }
#else
        /// <summary>
        /// Clears any saved credentials in the Locker and sets the current user to null.
        /// </summary>
        public static StatusCode DeleteSavedUser()
        {
            if (IsBusy == true)
                return StatusCode.InvalidRequest;
            
            IsBusy = true;
            CurrentUser = null;
            try
            {
                PasswordCredential credential = GetStoredCredential();
                new PasswordVault().Remove(credential);
            }
            catch { }
            IsBusy = false;
            return StatusCode.Success;
        }
#endif

        /// <summary>
        /// Refreshes and assigns the user details by requesting fresh data from the server. On success, the data is also cached before the function returns.
        /// </summary>
        /// <returns>
        /// Returns a status code containing information about the request's result.
        /// </returns>
        public static async Task<StatusCode> RefreshFromServerAsync()
        {
            return await MonitoredTask(async () =>
            {
                if (CurrentUser == null)
                    return StatusCode.InvalidRequest;

                try
                {
                    Response<string> response = await NetworkService.TryGetDataAsync(CurrentUser);
                    if (response.Code != StatusCode.Success)
                        return response.Code;

                    User temp = JsonParser.TryParseData(response.Content);
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
            });
        }

        /// <summary>
        /// Assigns the user details by reading data from the cache.
        /// </summary>
        /// <returns>
        /// Status code indicating result of operation.
        /// </returns>
        public static async Task<StatusCode> LoadCacheAsync()
        {
            return await MonitoredTask(async () =>
            {
                try
                {
                    if (CurrentUser == null)
                        return StatusCode.InvalidRequest;

                    if (string.Equals(CachedDataOwner, CurrentUser.RegNo, StringComparison.OrdinalIgnoreCase) == false)
                        return StatusCode.NoData;

                    StorageFile file = await _folder.GetFileAsync(JSON_FILE_NAME);
                    string jsonString = await StorageHelper.TryReadAsync(file);
                    User temp = JsonParser.TryParseData(jsonString);
                    if (temp == null)
                        return StatusCode.UnknownError;

                    CurrentUser = temp;
                    return StatusCode.Success;
                }
                catch
                {
                    return StatusCode.NoData;
                }
            });
        }

        #endregion

    }
}
