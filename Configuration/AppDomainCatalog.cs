// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Its.Configuration
{
    /// <summary>
    /// A catalog for all of the types found in all assemblies in the application's physical deployment location.
    /// </summary>
    public class AppDomainCatalog : AssembliesCatalog
    {
        protected override IEnumerable<Assembly> FindAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => !a.IsDynamic)
                .Where(a => !a.GlobalAssemblyCache);
        }
    }
}