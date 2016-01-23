using System;
using System.Collections.Generic;
using System.Text;
using Academics.SystemMetadata;
using Academics.ContentService;
using Academics.DataModel;
using System.Reflection;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.Storage;

namespace VITacademics.Managers
{
    public static class MetadataProvider
    {
        private const string SYSTEM_INFO_FILE_NAME = "systeminfo.txt";
        private static StorageFolder _localFolder = ApplicationData.Current.LocalFolder;

        private static bool _isRefreshing = false;

        public static string AppVersion
        {
            get
            {
                PackageVersion version = Package.Current.Id.Version;
                return String.Format("v{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);
            }
        }

        public static string AppId
        {
            get
            {
                return Windows.ApplicationModel.Store.CurrentApp.AppId.ToString();
            }
        }

        public static async Task<Response<List<Contributor>>> RequestContributorsFromSystemAsync()
        {
            if (_isRefreshing == true)
                return new Response<List<Contributor>>(StatusCode.InvalidRequest, null);

            _isRefreshing = true;
            List<Contributor> contributors = null;
            StatusCode code;

            Response<string> response = await NetworkService.TryGetSystemInfoAsync();
            if (response.Code == StatusCode.Success)
            {
                contributors = JsonParser.TryGetContributors(response.Content);
                if (contributors == null)
                {
                    code = StatusCode.UnknownError;
                    goto end;
                }
                else
                {
                    StorageFile file = await _localFolder.CreateFileAsync(SYSTEM_INFO_FILE_NAME, CreationCollisionOption.ReplaceExisting);
                    await StorageHelper.TryWriteAsync(file, response.Content);
                    goto end;
                }
            }
            else
            {
                code = response.Code;
            }

        end:
            _isRefreshing = false;
            return new Response<List<Contributor>>(response.Code, contributors);
        }

        public static async Task<Response<List<Contributor>>> GetContributesFromCacheAsync()
        {
            List<Contributor> contributors = null;
            try
            {
                StorageFile file = await _localFolder.GetFileAsync(SYSTEM_INFO_FILE_NAME);
                string content = await StorageHelper.TryReadAsync(file);
                contributors = JsonParser.TryGetContributors(content);
                if (contributors != null)
                    return new Response<List<Contributor>>(StatusCode.Success, contributors);
                else
                    throw new Exception();
            }
            catch
            {
                return new Response<List<Contributor>>(StatusCode.NoData, null);
            }
        }

    }
}
