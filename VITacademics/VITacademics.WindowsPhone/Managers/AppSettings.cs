using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VITacademics.Helpers;
using Windows.Storage;

namespace VITacademics.Managers
{
    public static class AppSettings
    {
        private const string APP_SETTINGS_CONTAINER_NAME = "appSettings";
        private const string AUTO_REFRESH_KEY = "autoRefresh";
        private const string DEFAULT_CONTROL_KEY = "defaultControlCode";

        private static ApplicationDataContainer _settingsContainer;

        static AppSettings()
        {
           _settingsContainer = ApplicationData.Current.RoamingSettings.CreateContainer(APP_SETTINGS_CONTAINER_NAME, ApplicationDataCreateDisposition.Always);
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
            }
        }
        public static ControlTypeCodes DefaultControlType
        {
            get
            {
                object val = _settingsContainer.Values[DEFAULT_CONTROL_KEY];
                if (val == null)
                    return ControlTypeCodes.Overview;
                else
                    return (ControlTypeCodes)(int)val;
            }
            set
            {
                _settingsContainer.Values[DEFAULT_CONTROL_KEY] = (int)value;
            }
        }

        public static void ResetToDefaults()
        {
            _settingsContainer.Values.Clear();
        }

    }
}
