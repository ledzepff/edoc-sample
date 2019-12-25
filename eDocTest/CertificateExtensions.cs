using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace eDocTest
{
    static class CertificateExtensions
    {
        public static void SetPinForPrivateKey(this X509Certificate2 certificate, string pin)
        {
            if (certificate == null) throw new ArgumentNullException("certificate");
            var key = (RSACryptoServiceProvider)certificate.PrivateKey;

            var handle = IntPtr.Zero;
            var pinBuff = Encoding.ASCII.GetBytes(pin);

            CryptNativeMethods.Execute(() => CryptNativeMethods.CryptAcquireContext(ref handle, key.CspKeyContainerInfo.KeyContainerName, key.CspKeyContainerInfo.ProviderName, key.CspKeyContainerInfo.ProviderType, CryptNativeMethods.CryptContextFlags.Silent));
            CryptNativeMethods.Execute(() => CryptNativeMethods.CryptSetProvParam(handle, CryptNativeMethods.CryptParameter.KeyExchangePin, pinBuff, 0));
            CryptNativeMethods.Execute(() => CryptNativeMethods.CertSetCertificateContextProperty(certificate.Handle, CryptNativeMethods.CertificateProperty.CryptoProviderHandle, 0, handle));
        }
    }
}
