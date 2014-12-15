// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;

namespace Its.Configuration.Tests
{
    [Export]
    public class SomethingConfigurableHavingStaticProperties
    {
        [Import("some_date")]
        public DateTime ADate { get; set; }

        [Import("some_uri")]
        public static Uri SomeUri { get; set; }
    }
}