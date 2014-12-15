// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Its.Configuration
{
    internal class AnonymousSettingsSource : ISettingsSource
    {
        private readonly GetSerializedSetting getSetting;
        private readonly string name;

        public AnonymousSettingsSource(GetSerializedSetting getSetting, string name = null)
        {
            if (getSetting == null)
            {
                throw new ArgumentNullException("getSetting");
            }
            this.getSetting = getSetting;
            this.name = name;
        }

        public string GetSerializedSetting(string key)
        {
            return getSetting(key);
        }

        public string Name
        {
            get
            {
                return name ?? getSetting.ToString();
            }
        }
    }
}