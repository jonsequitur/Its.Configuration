// Copyright (c) Microsoft. All rights reserved. 
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Its.Configuration
{
    /// <summary>
    /// Provides simplified methods for encryption and decryption using <see cref="EnvelopedCms" />.
    /// </summary>
    public static class CryptographyExtensions
    {
        /// <summary>
        /// Encrypts the specified string.
        /// </summary>
        /// <param name="plaintext">The plaintext to be encrypted.</param>
        /// <param name="certificate">The certificate to be used for encryption.</param>
        /// <returns>The encrypted text.</returns>
        public static string Encrypt(this string plaintext, X509Certificate2 certificate)
        {
            var contentInfo = new ContentInfo(Encoding.UTF8.GetBytes(plaintext));
            var envelopedCms = new EnvelopedCms(contentInfo);

            var cmsRecipient = new CmsRecipient(certificate);
            envelopedCms.Encrypt(cmsRecipient);

            return Convert.ToBase64String(envelopedCms.Encode());
        }

        /// <summary>
        /// Decrypts the specified string.
        /// </summary>
        /// <param name="ciphertext">The ciphertext to be decrypted.</param>
        /// <param name="certificates">A set of certificates containing the one that was used to encrypt the ciphertext.</param>
        /// <returns>The decrypted text.</returns>
        public static string Decrypt(this string ciphertext, params X509Certificate2[] certificates)
        {
            var certCollection = new X509Certificate2Collection(Settings.GetCertificatesFromStore().ToArray());

            if (certificates != null && certificates.Length > 0)
            {
                certCollection.AddRange(certificates);
            }

            var envelopedCms = new EnvelopedCms();
            envelopedCms.Decode(Convert.FromBase64String(ciphertext));
            envelopedCms.Decrypt(certCollection);
            return Encoding.UTF8.GetString(envelopedCms.ContentInfo.Content);
        }
    }
}