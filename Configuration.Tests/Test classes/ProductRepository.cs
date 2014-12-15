// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;

namespace Its.Configuration.Tests
{
    [Export]
    public class ProductRepository
    {
        [Import("products-db-connection-string")]
        public string ConnectionString { get; set; }

        [Import("db-retry-count")]
        public int RetryCount { get; set; }
    }
}