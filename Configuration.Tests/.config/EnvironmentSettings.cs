// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

namespace Its.Configuration.Tests
{
    public partial class EnvironmentSettings
    {
        public string Name { get; set; }
        public bool IsLocal { get; set; }
        public bool IsTest { get; set; }
    }
}