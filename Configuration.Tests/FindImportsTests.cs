// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Its.Configuration.Tests
{
    [TestFixture]
    public class FindImportsTests
    {
        [Test]
        public void Can_get_config_value_definitions_from_imports_defined_on_a_single_type()
        {
            var catalog = new TypeCatalog(typeof (SomethingConfigurable));
            var contractNamesAndTypes = catalog.FindImports();

            Assert.That(contractNamesAndTypes.Any(c => c.Id == "System.Boolean:some_bool" && c.ExpectedType == typeof (bool)));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == "System.Int32:some_int" && c.ExpectedType == (typeof (int))));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == "System.String:some_string" && c.ExpectedType == (typeof (string))));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == "System.DateTime:some_date" && c.ExpectedType == (typeof (DateTime))));
        }

        [Test]
        public void Can_get_config_value_definitions_from_imports_defined_on_a_single_static_type()
        {
            var catalog = new AppDomainCatalog();

            var imports = catalog.FindImports();
            var configDescription = imports
                .Where(d => d.DeclaringType == typeof (SomethingConfigurableAndStatic))
                .Single(d => d.Name == "some_other_int");

            Assert.That(configDescription.ExpectedType, Is.EqualTo(typeof (int)));
        }

        [Test]
        public void Can_get_config_value_definitions_from_imports_defined_on_static_members_of_non_static_types()
        {
            var imports = new[] { typeof (SomethingConfigurableHavingStaticProperties) }
                .FindImports();

            Assert.That(imports.Any(c => c.Id == typeof (Uri).ConfigKey("some_uri") && c.ExpectedType == (typeof (Uri))));
        }

        [Test]
        public void Can_get_config_value_definitions_when_multiple_instances_of_a_contract_occur_having_the_same_type()
        {
            var catalog = new AssemblyCatalog(typeof (SomethingConfigurable).Assembly);
            var contractNamesAndTypes = catalog.FindImports();

            Assert.That(contractNamesAndTypes.Any(c => c.Id == typeof (bool).ConfigKey("some_bool") && c.ExpectedType == (typeof (bool))));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == typeof (int).ConfigKey("some_int") && c.ExpectedType == (typeof (int))));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == typeof (string).ConfigKey("some_string") && c.ExpectedType == (typeof (string))));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == typeof (DateTime).ConfigKey("some_date") && c.ExpectedType == (typeof (DateTime))));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == typeof (string).ConfigKey("orders-db-connection-string") && c.ExpectedType == (typeof (string))));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == typeof (string).ConfigKey("products-db-connection-string") && c.ExpectedType == (typeof (string))));
            Assert.That(contractNamesAndTypes.Any(c => c.Id == typeof (int).ConfigKey("db-retry-count") && c.ExpectedType == (typeof (int))));
        }

        [Export]
        public class Widgetizer<T>
        {
            [Import("widgetizing-process-name")]
            public T WidgetizingProcess { get; set; }
        }
    }
}