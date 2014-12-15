// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Its.Configuration
{
    /// <summary>
    /// Gets a raw value from configuration.
    /// </summary>
    /// <param name="key">The key for the configuration value.</param>
    public delegate object GetConfigurationValue(string key);
}