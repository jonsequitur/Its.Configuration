// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.ComponentModel.Composition;
using System.IO;

namespace Its.Configuration.Tests
{
    [Export]
    public class SomethingConfigurableWithEnumMembers
    {
        [Import("file-attributes")]
        public FileAttributes FileAttributes;

        [Import("file-access")]
        public FileAccess? FileAccess;
    }
}