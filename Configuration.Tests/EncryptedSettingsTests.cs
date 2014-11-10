using System;
using System.Configuration;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using NUnit.Framework;

namespace Its.Configuration.Tests
{
    [TestFixture]
    public class EncryptedSettingsTests
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
        public void CryptographyExtensions_Encrypt_and_Decrypt_can_round_trip_a_string()
        {
            Settings.Precedence = new[] { "file-based-crypto-test" };

            var pfx = Settings.GetFile(f => f.Extension == ".pfx");

            Console.WriteLine("using " + pfx.FullName);

            var cert = new X509Certificate2(pfx.FullName, "password");

            var sooperSeekrit = new
            {
                Username = "HarryHoud1n1",
                Password = "0p3ns3s@m3"
            }.ToJson();

            var ciphertext = sooperSeekrit.Encrypt(cert);

            Console.WriteLine(ciphertext);

            ciphertext.Decrypt(cert)
                      .Should().Be(sooperSeekrit);
        }

        [Test]
        public void Encrypted_settings_are_decrypted_using_available_certificates_from_config_folder()
        {
            Settings.Precedence = new[] { "file-based-crypto-test" };
            Settings.CertificatePassword = file =>
            {
                var s = new SecureString();
                foreach (var c in "password")
                {
                    s.AppendChar(c);
                }
                return s;
            };

            // this actually resolves from Credentials.json.secure
            var credentials = Settings.Get<SettingsTests.Credentials>();

            credentials.Username.Should().Be("HarryHoud1n1");
            credentials.Password.Should().Be("0p3ns3s@m3");
        }

        [Test]
        public void When_two_settings_files_have_the_same_base_name_then_an_exception_is_thrown()
        {
            Settings.Precedence = new[] { "file-based-crypto-collision-test" };

            // this actually resolves from Credentials.json.secure
            Action getSettings = () => Settings.Get<SettingsTests.Credentials>();

            getSettings.ShouldThrow<ConfigurationErrorsException>()
                       .And
                       .Message.Should().Contain(@".config\file-based-crypto-collision-test\Credentials");
        }

        [Ignore("Test not finished")]
        [Test]
        public void Encrypted_settings_are_decrypted_using_available_certificates_from_the_machine_store()
        {
            // TODO: (Encrypted_settings_are_decrypted_using_available_certificates_from_the_machine_store) figure out how to test this
            Settings.Precedence = new[] { "certificate-store-based-crypto-test" };

            // this actually resolves from Credentials.json.secure
            var credentials = Settings.Get<SettingsTests.Credentials>();

            credentials.Username.Should().Be("HarryHoud1n1");
            credentials.Password.Should().Be("0p3ns3s@m3");
        }

    }
}