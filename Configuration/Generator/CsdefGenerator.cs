// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Its.Configuration.Generator
{
    internal class CsdefGenerator
    {
        public string Generate(IEnumerable<Assembly> assemblies)
        {
            var imports = assemblies.FindImports().ToArray();

            return imports
                .OrderBy(import => import.Name)
                .Distinct(new SettingNameEqualityComparer())
                .Select(import =>
                        string.Format(
                            "\n<!-- Expected types: {1} -->\n<Setting name=\"{0}\" />",
                            import.Name,
                            imports.Where(i => i.Name == import.Name)
                                .Select(i => i.ExpectedType.ToString())
                                .Distinct()
                                .OrderBy(name => name)
                                .ToDelimitedString(", ")))
                .ToDelimitedString("\n");
        }

        private class SettingNameEqualityComparer : IEqualityComparer<ConfigurationDescription>
        {
            public bool Equals(ConfigurationDescription x, ConfigurationDescription y)
            {
                return Equals(x.Name, y.Name);
            }

            public int GetHashCode(ConfigurationDescription description)
            {
                return description.Name.GetHashCode();
            }
        }
    }
}