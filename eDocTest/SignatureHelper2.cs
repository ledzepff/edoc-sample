using PostCSP.eDocLib;
using PostCSP.eDocLib.Configuration;
using PostCSP.Util.Asn1;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace eDocTest
{
    public static class SignatureHelper2
    {
        // Token: 0x0600026B RID: 619 RVA: 0x0000FD2C File Offset: 0x0000DF2C
        internal static string BuildChainErrors(X509Chain ch)
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (X509ChainElement x509ChainElement in ch.ChainElements)
            {
                foreach (X509ChainStatus x509ChainStatus in x509ChainElement.ChainElementStatus)
                {
                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.Append(";");
                    }
                    stringBuilder.AppendFormat("{0}: {1} ({2})\r\n", x509ChainElement.Certificate.Subject, x509ChainStatus.Status.ToString(), x509ChainStatus.StatusInformation);
                }
            }
            return stringBuilder.ToString();
        }

        // Token: 0x0600026C RID: 620 RVA: 0x0000FDF4 File Offset: 0x0000DFF4
        internal static bool IsTrustedRoot(X509Certificate2 cert)
        {
            X509Store x509Store = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            x509Store.Open(OpenFlags.OpenExistingOnly);
            X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindBySerialNumber, cert.SerialNumber, false);
            if (x509Certificate2Collection.Count == 0)
            {
                x509Store = new X509Store(StoreName.Root, StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.OpenExistingOnly);
                x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindBySerialNumber, cert.SerialNumber, false);
            }
            return x509Certificate2Collection.Count != 0 && x509Certificate2Collection[0].Verify();
        }

        // Token: 0x0600026D RID: 621 RVA: 0x0000FE84 File Offset: 0x0000E084
        internal static bool IsIssuer(X509Certificate2 possibleIssuer, X509Certificate2 cert)
        {
            Asn1Block asn1Block = new Asn1Block(cert.RawData, cert.RawData.Length);
            asn1Block.SkipTag();
            byte[] buffer = asn1Block.ReadWithTag(Asn1Tag.Sequence | Asn1Tag.Complex);
            Asn1SequenceDecoder asn1SequenceDecoder = new Asn1SequenceDecoder(asn1Block);
            ObjectId algOid = asn1Block.ReadOid();
            asn1SequenceDecoder.End();
            int num;
            int num2;
            asn1Block.Get(Asn1Tag.BitString, out num, out num2);
            byte[] bytes = asn1Block.GetBytes(num + 1, num2 - 1);
            AsymmetricAlgorithm key = possibleIssuer.PublicKey.Key;
            RSACryptoServiceProvider rsacryptoServiceProvider = key as RSACryptoServiceProvider;
            bool result;
            if (rsacryptoServiceProvider == null)
            {
                result = false;
            }
            else
            {
                using (HashAlgorithm hashAlgorithm = SignatureHelper2.MakeHashAlgorithm(algOid))
                {
                    if (!rsacryptoServiceProvider.VerifyData(buffer, hashAlgorithm, bytes))
                    {
                        return false;
                    }
                }
                result = true;
            }
            return result;
        }

        // Token: 0x0600026E RID: 622 RVA: 0x0000FF64 File Offset: 0x0000E164
        private static bool IsRightUsage(X509Extension ext, string usage)
        {
            bool result;
            if (usage == "*")
            {
                result = true;
            }
            else
            {
                X509KeyUsageExtension x509KeyUsageExtension = ext as X509KeyUsageExtension;
                if (x509KeyUsageExtension != null)
                {
                    try
                    {
                        X509KeyUsageFlags x509KeyUsageFlags = (X509KeyUsageFlags)Enum.Parse(typeof(X509KeyUsageFlags), usage, true);
                        if ((x509KeyUsageExtension.KeyUsages & x509KeyUsageFlags) == x509KeyUsageFlags)
                        {
                            return true;
                        }
                    }
                    catch
                    {
                    }
                }
                X509EnhancedKeyUsageExtension x509EnhancedKeyUsageExtension = ext as X509EnhancedKeyUsageExtension;
                if (x509EnhancedKeyUsageExtension != null)
                {
                    foreach (Oid oid in x509EnhancedKeyUsageExtension.EnhancedKeyUsages)
                    {
                        if (oid.Value == usage)
                        {
                            return true;
                        }
                    }
                }
                result = false;
            }
            return result;
        }

        // Token: 0x0600026F RID: 623 RVA: 0x00010048 File Offset: 0x0000E248
        public static bool IsRightUsage(X509Certificate2 cert, string usages)
        {
            string[] array = usages.Split(new char[]
            {
                ','
            });
            foreach (X509Extension ext in cert.Extensions)
            {
                bool flag = true;
                foreach (string usage in array)
                {
                    flag = (flag && SignatureHelper2.IsRightUsage(ext, usage));
                }
                if (flag)
                {
                    return true;
                }
            }
            return false;
        }

        // Token: 0x06000270 RID: 624 RVA: 0x000100E4 File Offset: 0x0000E2E4
        public static bool IsRightUsageSigning(X509Certificate2 cert)
        {
            EDocLibAuthorityConfig authorityConfiguration = EdocLibConfig.Instance.GetAuthorityConfiguration(cert.Issuer);
            bool flag = authorityConfiguration != null;
            if (flag)
            {
                flag = SignatureHelper2.IsRightUsage(cert, EdocLibConfig.Instance.KeyUsagesSigning);
            }
            return flag;
        }

        // Token: 0x06000271 RID: 625 RVA: 0x0001012C File Offset: 0x0000E32C
        public static bool IsRightUsageAuthentication(X509Certificate2 cert)
        {
            EDocLibAuthorityConfig authorityConfiguration = EdocLibConfig.Instance.GetAuthorityConfiguration(cert.Issuer);
            bool flag = authorityConfiguration != null && !string.IsNullOrEmpty(authorityConfiguration.TsaResponderUrl);
            if (flag)
            {
                flag = SignatureHelper.IsRightUsage(cert, EdocLibConfig.Instance.KeyUsagesAuthentication);
            }
            return flag;
        }

        // Token: 0x06000272 RID: 626 RVA: 0x00010184 File Offset: 0x0000E384
        public static RSACryptoServiceProvider GetCertificatePrivateKey(X509Certificate2 signerCert)
        {
            if (!signerCert.HasPrivateKey)
            {
                throw new EDocException(EDocMessages.CertificateDoesNotSupportSigning);
            }
            DateTime now = DateTime.Now;
            if (now > signerCert.NotAfter || now < signerCert.NotBefore)
            {
                throw new EDocException(EDocMessages.SigningCertificateExpired);
            }
            if (!SignatureHelper.IsRightUsage(signerCert, EdocLibConfig.Instance.KeyUsagesAuthentication) && !SignatureHelper.IsRightUsage(signerCert, EdocLibConfig.Instance.KeyUsagesSigning))
            {
                throw new EDocException(EDocMessages.CertificateDoesNotSupportSigning);
            }
            return SignatureHelper2.DoGetPrivateKey(signerCert);
        }

        // Token: 0x06000273 RID: 627 RVA: 0x00010214 File Offset: 0x0000E414
        private static RSACryptoServiceProvider DoGetPrivateKey(X509Certificate2 signerCert)
        {
            RSACryptoServiceProvider result;
            try
            {
                RSACryptoServiceProvider rsacryptoServiceProvider = signerCert.PrivateKey as RSACryptoServiceProvider;
                if (rsacryptoServiceProvider == null)
                {
                    throw new EDocException(EDocMessages.CertificateDoesNotSupportSigning);
                }
                if (rsacryptoServiceProvider.CspKeyContainerInfo.HardwareDevice)
                {
                    CspKeyContainerInfo cspKeyContainerInfo = rsacryptoServiceProvider.CspKeyContainerInfo;
                    result = new RSACryptoServiceProvider(new CspParameters(cspKeyContainerInfo.ProviderType, cspKeyContainerInfo.ProviderName, cspKeyContainerInfo.UniqueKeyContainerName)
                    {
                        KeyNumber = 2
                    });
                }
                else
                {
                    result = rsacryptoServiceProvider;
                }
            }
            catch (EDocException)
            {
                throw;
            }
            catch (Exception ex)
            {
                eDocUtil.ReportException(ex.ToString());
                throw new EDocException(EDocMessages.CantObtainPrivateKey);
            }
            return result;
        }

        // Token: 0x06000274 RID: 628 RVA: 0x000102C8 File Offset: 0x0000E4C8
        internal static bool CompareArray<T>(T[] a1, T[] a2)
        {
            bool result;
            if (a1.Length != a2.Length)
            {
                result = false;
            }
            else
            {
                for (int i = 0; i < a1.Length; i++)
                {
                    if (!a1[i].Equals(a2[i]))
                    {
                        return false;
                    }
                }
                result = true;
            }
            return result;
        }

        // Token: 0x06000275 RID: 629 RVA: 0x00010328 File Offset: 0x0000E528
        internal static HashAlgorithm MakeHashAlgorithm(ObjectId algOid)
        {
            HashAlgorithm result;
            if (algOid.Equals(ObjectId.Sha1))
            {
                result = new SHA1Managed();
            }
            else if (algOid.Equals(ObjectId.RsaWithSha1))
            {
                result = new SHA1Managed();
            }
            else
            {
                result = null;
            }
            return result;
        }

        // Token: 0x06000276 RID: 630 RVA: 0x00010370 File Offset: 0x0000E570
        internal static bool VerifySignature(byte[] hash, ObjectId hashAlgOid, X509Certificate2 signingCert, byte[] signature)
        {
            using (AsymmetricAlgorithm key = signingCert.PublicKey.Key)
            {
                RSACryptoServiceProvider rsacryptoServiceProvider = key as RSACryptoServiceProvider;
                if (!rsacryptoServiceProvider.VerifyHash(hash, hashAlgOid.ToString(), signature))
                {
                    return false;
                }
            }
            return true;
        }

        // Token: 0x0400011C RID: 284
        internal const string ExtendedKeyUsageOid = "2.5.29.37";

        // Token: 0x0400011D RID: 285
        internal const string ClientAuthenticationOid = "1.3.6.1.5.5.7.3.2";
    }
}
