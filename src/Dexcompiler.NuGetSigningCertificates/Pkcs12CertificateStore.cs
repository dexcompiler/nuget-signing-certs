using System.Security.Cryptography.X509Certificates;

namespace Dexcompiler.NuGetSigningCertificates;

public static class Pkcs12CertificateStore
{
    public static byte[] Export(X509Certificate2 certificate, ReadOnlySpan<char> password)
    {
        ArgumentNullException.ThrowIfNull(certificate);

        if (!certificate.HasPrivateKey)
            throw new ArgumentException("Certificate must contain a private key.", nameof(certificate));
        if (password.IsEmpty)
            throw new ArgumentException("Password must not be empty.", nameof(password));

        return certificate.Export(X509ContentType.Pkcs12, password.ToString());
    }

    public static X509Certificate2 Import(
        ReadOnlySpan<byte> pfx,
        ReadOnlySpan<char> password,
        X509KeyStorageFlags keyStorageFlags = X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet)
    {
        if (pfx.IsEmpty)
            throw new ArgumentException("PFX must not be empty.", nameof(pfx));
        if (password.IsEmpty)
            throw new ArgumentException("Password must not be empty.", nameof(password));

        return CertificateLoaderCompat.LoadPkcs12(pfx, password, keyStorageFlags);
    }
}

