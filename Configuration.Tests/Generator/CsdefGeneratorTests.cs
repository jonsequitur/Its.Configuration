// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Xml.Linq;
using Its.Configuration.Generator;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace Its.Configuration.Tests.Generator
{
    [TestFixture]
    public class CsdefGeneratorTests
    {
        [Test]
        public void Generates_one_Setting_element_per_unique_Import_contract_name()
        {
            var generator = new CsdefGenerator();
            var assemblies = new DeploymentCatalog().Assemblies().ToArray();

            var result = generator.Generate(assemblies);

            Console.WriteLine(result);

            var xdoc = XDocument.Parse("<ConfigurationSettings>" + result + "</ConfigurationSettings>");
            Assert.That(
                xdoc.Elements("ConfigurationSettings").First().Elements("Setting").Count(),
                Is.EqualTo(assemblies.FindImports().Select(i => i.Name).Distinct().Count()));
        }

        [Test]
        public void Uses_Import_contract_names_as_key_names()
        {
            var generator = new CsdefGenerator();
            var assemblies = new DeploymentCatalog().Assemblies().ToArray();

            var result = generator.Generate(assemblies);

            var xdoc = XDocument.Parse("<ConfigurationSettings>" + result + "</ConfigurationSettings>");
            
            var nameAttributeValues = xdoc.Elements("ConfigurationSettings").First()
                .Elements("Setting")
                .Select(e => e.Attribute("name").Value)
                .OrderBy(n => n);
            var contractNames = assemblies.FindImports()
                .Select(i => i.Name)
                .Distinct()
                .OrderBy(n => n);
            Assert.That(nameAttributeValues.IsSameSequenceAs(contractNames));
        }
    }
}