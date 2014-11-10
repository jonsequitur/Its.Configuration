using System;

namespace Its.Configuration
{
    internal class EnvironmentVariableSettingsSource : ISettingsSource
    {
        public string GetSerializedSetting(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }

        public string Name
        {
            get
            {
                return "environment variable";
            }
        }
    }
}