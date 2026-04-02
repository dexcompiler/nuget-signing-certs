using System.Security.Cryptography.X509Certificates;

namespace Dexcompiler.NuGetSigningCertificates;

internal static class CertificateLoaderCompat
{
    public static X509Certificate2 LoadPkcs12(
        ReadOnlySpan<byte> pfx,
        ReadOnlySpan<char> password,
        X509KeyStorageFlags keyStorageFlags)
    {
#if NET9_0_OR_GREATER
        return X509CertificateLoader.LoadPkcs12(pfx, password, keyStorageFlags);
#else
        return new X509Certificate2(pfx.ToArray(), password.ToString(), keyStorageFlags);
#endif
    }
}

