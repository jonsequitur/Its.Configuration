// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Its.Configuration.Tests
{
    [TestFixture]
    public class SelfVerificationTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.Reset();
            Environment.SetEnvironmentVariable("Its.Configuration.Settings.Precedence", null);
            Settings.CertificatePassword = null;
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable("Its.Configuration.Settings.Precedence", null);
        }

        [Test]
        public void GetFiles_returns_only_files_in_the_specified_precedence_path_when_precedence_is_empty()
        {
            Settings.Precedence = new string[0];
            var files = Settings.GetFiles().ToArray();
            files.Count().Should().Be(2);
        }

        [Test]
        public void GetFiles_returns_only_files_in_the_specified_precedence_path_when_precedence_is_single_folder()
        {
            Settings.Precedence = new[] { "test" };
            var files = Settings.GetFiles().ToArray();
            files.Count().Should().Be(2);
            files.Should().ContainSingle(f => f.Name == "OnlyConfiguredInRootFolder.json");
            files.Single(f => f.Name == "EnvironmentSettings.json")
                 .FullName.Should().Contain(@"\test\");
        }

        [Test]
        public void GetFiles_returns_only_files_in_the_specified_precedence_path_when_precedence_is_multi_folder()
        {
            // subfolder + root
            Settings.Precedence = new[] { "test", "production", "internal", "file-based-crypto-test" };
            var files = Settings.GetFiles().ToArray();
            files.Count().Should().Be(5);
            files.Single(f => f.Name == "EnvironmentSettings.json")
                 .FullName.Should().Contain(@"\test\");
            files.Single(f => f.Name == "DbConnectionSettings.json")
                 .FullName.Should().Contain(@"\production\");
        }
    }
}