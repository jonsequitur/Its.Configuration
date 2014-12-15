// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;

namespace Its.Configuration
{
    internal class ConfigurationDescription
    {
        // TODO: (ConfigurationDescription) finish this
        public string Id { get; set; }
        public string Name { get; set; }
        public Type DeclaringType { get; set; }
        public Type ExpectedType { get; set; }
        public bool AllowDefault { get; set; }
    }
}