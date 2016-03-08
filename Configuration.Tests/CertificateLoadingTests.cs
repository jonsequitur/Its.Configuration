using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;

namespace Its.Configuration.Tests
{
    [TestFixture]
    public class CertificateLoadingTests
    {
        [SetUp]
        public void SetUp()
        {
            Settings.CertificatePassword = file =>
            {
                var s = new SecureString();
                foreach (var c in "password")
                {
                    s.AppendChar(c);
                }
                return s;
            };

            Settings.Precedence = new[]
            {
                "file-based-crypto-test"
            };

            var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            store.Open(OpenFlags.ReadWrite);
            if (store.Certificates.Find(X509FindType.FindByThumbprint, "20a9ec8611f70b3cc2c4805baed626f6f02d5bf1", false).Count == 0) ;
            {
                var cert = new X509Certificate2("ItsConfigurationInstalledInStoreTest.pfx", "password");
                store.Add(cert); //where cert is an X509Certificate object
            }
            store.Close();
        }
        
        [TearDown]
        public void TearDown()
        {
            Settings.Reset();
        }

        [Test]
        public void Certificate_can_be_pulled_from_cert_store()
        {
            var cert = Settings.GetCertificatesFromStore(StoreLocation.CurrentUser, StoreName.My)
                               .SingleOrDefault(c => c.Subject.Equals("CN=ItsConfigurationInstalledInStoreTest",
                                   StringComparison.OrdinalIgnoreCase));
            cert.Should()
                .NotBeNull();
        }


        [Test]
        public void Certificate_can_be_pulled_from_file()
        {
            var cert = Settings.GetCertificatesFromConfigDirectory()
                               .SingleOrDefault(c => c.Subject.Equals("CN=ItsConfigurationTest", StringComparison.OrdinalIgnoreCase));
            cert.Should().NotBeNull();
        }
    }
}
