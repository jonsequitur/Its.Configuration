// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;

namespace Its.Configuration.Tests
{
    [Export]
    public class SomethingConfigurable
    {
        [Import("some_date")]
        public DateTime ADate { get; set; }

        [Import("some_int")]
        public int AnInt { get; set; }

        [Import("some_string")]
        public string AString { get; set; }

        [Import("some_bool")]
        public bool ABool { get; set; }
    }
}