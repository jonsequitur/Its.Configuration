// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;

namespace Its.Configuration.Tests
{
    public static class SomethingConfigurableAndStatic
    {
        [Import("some_string")]
        internal static string a_string = "default value";

        [Import("some_bool")]
        private static bool a_bool = false;

        static SomethingConfigurableAndStatic()
        {
            AnIntProperty = 5;
        }

        [Import("some_date")]
        public static DateTime ADateProperty { get; set; }

        [Import("some_other_int", AllowDefault = true)]
        public static int AnIntProperty { get; set; }

        [Import("some_date")]
        public static DateTime ADateField;

        [Import("some_int", AllowDefault = true)]
        public static int AnIntField = 2457;

        public static bool APrivateBoolFieldsAccessorProperty
        {
            get
            {
                return a_bool;
            }
        }

        public static string AnInternalStringFieldsAccessorProperty
        {
            get
            {
                return a_string;
            }
        }
    }
}