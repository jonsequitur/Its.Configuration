using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using NUnit.Framework;

namespace Its.Configuration.Console.Tests
{
    [TestFixture]
    public class EncryptTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.Precedence = new[] { "file-based-crypto-test" };
        }

        [Test]
        public void The_certificate_can_be_a_file_name()
        {
            var plaintext = "hello there";
            var file = MethodBase.GetCurrentMethod().Name + ".txt";

            File.WriteAllText(file, plaintext);

            var certificate = Settings.GetFile(f => f.Extension == ".pfx").FullName;
            var encrypted = Program.Encrypt(new ConsoleParameters
            {
                FileSpec = file,
                Certificate = certificate,
                Password = "password"
            });

            var decrypted = encrypted.Decrypt(new X509Certificate2(certificate, "password"));

            decrypted.Should().Be(plaintext);
        }
    }
}