// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Its.Configuration
{
    /// <summary>
    ///     Accesses a settings string corresponding to the specified key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>A string representing serialized settings.</returns>
    public delegate string GetSerializedSetting(string key);
}