using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Dexcompiler.NuGetSigningCertificates.Tests;

public sealed class NuGetSigningCertificateValidatorTests
{
    [Fact]
    public void Validate_WithValidGeneratedCertificate_ShouldReturnValid()
    {
        using X509Certificate2 cert = CodeSigningCertificateGenerator.CreateSelfSignedCertificate(
            new CodeSigningCertificateRequest
            {
                SubjectName = "CN=validator-valid"
            });

        CertificateValidationResult result = NuGetSigningCertificateValidator.Validate(cert);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void Validate_WithMissingCodeSigningEku_ShouldReturnIssue()
    {
        using RSA rsa = RSA.Create(3072);
        var request = new CertificateRequest(
            "CN=no-code-signing",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        request.CertificateExtensions.Add(
            new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, critical: true));

        using X509Certificate2 cert = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-5), DateTimeOffset.UtcNow.AddDays(30));
        CertificateValidationResult result = NuGetSigningCertificateValidator.Validate(cert);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "EKU_MISSING");
    }
}

