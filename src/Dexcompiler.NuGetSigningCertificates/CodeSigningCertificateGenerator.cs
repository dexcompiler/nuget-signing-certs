using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Dexcompiler.NuGetSigningCertificates;

public static class CodeSigningCertificateGenerator
{
    public static X509Certificate2 CreateSelfSignedCertificate(CodeSigningCertificateRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.SubjectName))
            throw new ArgumentException("SubjectName must not be empty.", nameof(request));

        if (request.KeySizeInBits < NuGetSigningCertificateRequirements.MinimumRsaKeySizeInBits)
            throw new ArgumentOutOfRangeException(nameof(request), $"KeySizeInBits must be >= {NuGetSigningCertificateRequirements.MinimumRsaKeySizeInBits}.");

        if (request.NotAfter <= request.NotBefore)
            throw new ArgumentException("NotAfter must be later than NotBefore.", nameof(request));

        if (request.HashAlgorithm != HashAlgorithmName.SHA256 &&
            request.HashAlgorithm != HashAlgorithmName.SHA384 &&
            request.HashAlgorithm != HashAlgorithmName.SHA512)
        {
            throw new ArgumentException("HashAlgorithm must be SHA256, SHA384, or SHA512.", nameof(request));
        }

        using RSA rsa = RSA.Create(request.KeySizeInBits);
        var distinguishedName = new X500DistinguishedName(request.SubjectName);
        var certRequest = new CertificateRequest(
            distinguishedName,
            rsa,
            request.HashAlgorithm,
            RSASignaturePadding.Pkcs1);

        certRequest.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        certRequest.CertificateExtensions.Add(
            new X509EnhancedKeyUsageExtension(
                [NuGetSigningCertificateRequirements.CodeSigningEkuOid],
                critical: true));

        certRequest.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(
                certificateAuthority: false,
                hasPathLengthConstraint: false,
                pathLengthConstraint: 0,
                critical: true));

        certRequest.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(certRequest.PublicKey, critical: false));

        using X509Certificate2 generated = certRequest.CreateSelfSigned(request.NotBefore, request.NotAfter);
        byte[] pfx = generated.Export(X509ContentType.Pkcs12);

        try
        {
            // Import into an exportable ephemeral key container for predictable behavior.
            return CertificateLoaderCompat.LoadPkcs12(
                pfx,
                ReadOnlySpan<char>.Empty,
                X509KeyStorageFlags.Exportable | X509KeyStorageFlags.EphemeralKeySet);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pfx);
        }
    }
}

