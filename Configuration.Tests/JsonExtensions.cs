// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Web.Script.Serialization;

namespace Its.Configuration.Tests
{
    public static class JsonExtensions
    {
        public static string ToJson(this object o)
        {
            return new JavaScriptSerializer().Serialize(o);
        }
    }
}