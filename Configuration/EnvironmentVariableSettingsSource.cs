// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

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