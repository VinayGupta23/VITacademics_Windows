using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using VITacademics.Helpers;
using VITacademics.UIControls;
using Windows.Storage;

namespace VITacademics.Managers
{
    public static class AppSettings
    {
        private const string APP_SETTINGS_CONTAINER_NAME = "appSettings";
        private const string AUTO_REFRESH_KEY = "autoRefresh";
        private const string DEFAULT_CONTROL_KEY = "defaultControlFullTypeName";
        private const string FIRST_RUN_KEY = "firstRun";
        private const string SEMESTER_UPGRADE_KEY = "semUpgradeAvailable";

        private static readonly ApplicationDataContainer _settingsContainer;

        public static event PropertyChangedEventHandler SettingsChanged;
        private static void NotifySettingChanged([CallerMemberName]string settingName = null)
        {
            if (SettingsChanged != null)
                SettingsChanged(null, new PropertyChangedEventArgs(settingName));
        }

        static AppSettings()
        {
            _settingsContainer = App._roamingSettings.CreateContainer(APP_SETTINGS_CONTAINER_NAME, ApplicationDataCreateDisposition.Always);
        }

        public static bool AutoRefresh
        {
            get
            {
                object val = _settingsContainer.Values[AUTO_REFRESH_KEY];
                if (val == null)
                    return false;
                else
                    return (bool)val;
            }
            set
            {
                _settingsContainer.Values[AUTO_REFRESH_KEY] = value;
                NotifySettingChanged();
            }
        }
        public static bool FirstRun
        {
            get
            {
                object val = _settingsContainer.Values[FIRST_RUN_KEY];
                if (val == null)
                    return true;
                else
                    return (bool)val;
            }
            set
            {
                _settingsContainer.Values[FIRST_RUN_KEY] = value;
                NotifySettingChanged();
            }
        }
        public static string DefaultControlTypeName
        {
            get
            {
                object val = _settingsContainer.Values[DEFAULT_CONTROL_KEY];
                if (val == null)
                    return typeof(UserOverviewControl).FullName;
                else
                    return val as string;
            }
            set
            {
                _settingsContainer.Values[DEFAULT_CONTROL_KEY] = value;
                NotifySettingChanged();
            }
        }
        public static bool IsSemesterUpgradeAvailable
        {
            get
            {
                object val = _settingsContainer.Values[SEMESTER_UPGRADE_KEY];
                if (val == null)
                    return false;
                else
                    return (bool)val;
            }
            set
            {
                _settingsContainer.Values[SEMESTER_UPGRADE_KEY] = value;
                NotifySettingChanged();
            }
        }

        public static void ResetToDefaults()
        {
            _settingsContainer.Values.Clear();
        }
    }
}
