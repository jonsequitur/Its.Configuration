// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using NUnit.Framework;

namespace Its.Configuration.Console.Tests
{
    [TestFixture]
    public class DecryptTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.Precedence = new[] { "file-based-crypto-test" };
        }

        [Test]
        public void The_certificate_can_be_a_file_name()
        {
            var file = MethodBase.GetCurrentMethod().Name + ".txt";
            var certFile = Settings.GetFile(f => f.Extension == ".pfx").FullName;
            var certificate = new X509Certificate2(certFile, "password");
            var ciphertext = "hello there".Encrypt(certificate);
            
            File.WriteAllText(file, ciphertext);

            var decrypted = Program.Decrypt(new ConsoleParameters
            {
                FileSpec = file,
                Certificate = certFile,
                Password = "password"
            });

            decrypted.Should().Be(decrypted);
        }
    }
}