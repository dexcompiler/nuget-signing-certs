using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Dexcompiler.NuGetSigningCertificates.Tests;

public sealed class CodeSigningCertificateGeneratorTests
{
    [Fact]
    public void CreateSelfSignedCertificate_WithDefaults_ShouldProduceNuGetCompatibleProfile()
    {
        var request = new CodeSigningCertificateRequest
        {
            SubjectName = "CN=nuget-signing-certs-test"
        };

        using X509Certificate2 cert = CodeSigningCertificateGenerator.CreateSelfSignedCertificate(request);

        Assert.True(cert.HasPrivateKey);
        Assert.Contains("CN=nuget-signing-certs-test", cert.Subject, StringComparison.Ordinal);

        using RSA? rsa = cert.GetRSAPublicKey();
        Assert.NotNull(rsa);
        Assert.True(rsa!.KeySize >= NuGetSigningCertificateRequirements.MinimumRsaKeySizeInBits);

        var result = NuGetSigningCertificateValidator.Validate(cert);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateSelfSignedCertificate_WithSmallKeySize_ShouldThrow()
    {
        var request = new CodeSigningCertificateRequest
        {
            SubjectName = "CN=small-key",
            KeySizeInBits = 1024
        };

        Assert.Throws<ArgumentOutOfRangeException>(() => CodeSigningCertificateGenerator.CreateSelfSignedCertificate(request));
    }

    [Fact]
    public void CreateSelfSignedCertificate_WithUnsupportedHash_ShouldThrow()
    {
        var request = new CodeSigningCertificateRequest
        {
            SubjectName = "CN=hash-test",
            HashAlgorithm = HashAlgorithmName.SHA1
        };

        Assert.Throws<ArgumentException>(() => CodeSigningCertificateGenerator.CreateSelfSignedCertificate(request));
    }
}

