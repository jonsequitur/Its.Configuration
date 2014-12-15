// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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