// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;

namespace Its.Configuration.Tests
{
    [Export]
    public static class SomethingConfigurableAndStaticHavingSpecialTypes
    {
        [Import("some_uri", AllowDefault = true)]
        public static Uri SomeUri { get; set; }
    }
}