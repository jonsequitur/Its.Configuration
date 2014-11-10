using System.Configuration;

namespace Its.Configuration
{
    internal class AppConfigSettingsSource : ISettingsSource
    {
        public string GetSerializedSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public string Name
        {
            get
            {
                return "web.config / app.config";
            }
        }
    }
}