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
        private const string RESOURCE_NAME = "VITacademics";

        private static StorageFolder Folder
        {
            get;
            set;
        }
        public static User CurrentUser
        {
            get;
            private set;
        }

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

        /// <summary>
        /// Assigns the current user and logs in if stored credentials are found and valid, otherwise assigns null. 
        /// </summary>
        /// <remarks>
        /// If the stored credentials are corrupted, the Credential Locker is cleared.
        /// </remarks>
        /// <returns>
        /// Indicates success if true is returned, status code contains login status.
        /// </returns>
        public static async Task<Response<bool>> TryLoginSavedUser()
        {
            PasswordCredential credential = GetStoredCredential();

            if (credential != null)
                try
                {
                    credential = new PasswordVault().Retrieve(RESOURCE_NAME, credential.UserName);
                    // Parse "password" to retrieve DOB and campus
                    DateTimeOffset dob = DateTimeOffset.ParseExact(credential.Password.Substring(0, 8), "ddMMyyyy", CultureInfo.InvariantCulture);
                    string campus = credential.Password.Substring(8);

                    CurrentUser = new User(credential.UserName, dob, campus);
                    Folder = ApplicationData.Current.LocalFolder;

                    StatusCode code = await NetworkService.TryLoginAsync(CurrentUser);
                    return new Response<bool>(code, true);
                }
                catch
                {
                    // The credential stored is corrupted.
                    DeleteSavedUser();
                }

            // Failed to find user.
            CurrentUser = null;
            Folder = null;
            return new Response<bool>(StatusCode.NoData, false);
        }

        /// <summary>
        /// Clears any saved credentials in the Locker and resets the current user to null.
        /// </summary>
        public static void DeleteSavedUser()
        {
            CurrentUser = null;
            Folder = null;
            try
            {
                PasswordCredential credential = GetStoredCredential();
                if (credential != null)
                    new PasswordVault().Remove(credential);
            }
            catch { }
        }

        /// <summary>
        /// Attempts to login using passed user credentials and assigns the current user on success. If login fails a specific status code is returned.
        /// </summary>
        /// <remarks>
        /// Note: Any existing credentials in the Locker are overwritten on success.
        /// </remarks>
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

                Folder = ApplicationData.Current.LocalFolder;
                CurrentUser = user;
            }

            return status;
        }

    }
}
