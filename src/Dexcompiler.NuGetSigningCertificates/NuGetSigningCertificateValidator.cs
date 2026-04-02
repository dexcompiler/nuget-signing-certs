using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace Dexcompiler.NuGetSigningCertificates;

public static class NuGetSigningCertificateValidator
{
    public static CertificateValidationResult Validate(
        X509Certificate2 certificate,
        DateTimeOffset? validationTime = null)
    {
        ArgumentNullException.ThrowIfNull(certificate);

        var issues = new List<CertificateValidationIssue>();
        DateTimeOffset now = validationTime ?? DateTimeOffset.UtcNow;

        ValidateValidityWindow(certificate, now, issues);
        ValidateRsaRequirements(certificate, issues);
        ValidateKeyUsage(certificate, issues);
        ValidateCodeSigningEku(certificate, issues);

        return new CertificateValidationResult
        {
            Issues = issues
        };
    }

    private static void ValidateValidityWindow(
        X509Certificate2 certificate,
        DateTimeOffset now,
        List<CertificateValidationIssue> issues)
    {
        DateTimeOffset notBefore = certificate.NotBefore;
        DateTimeOffset notAfter = certificate.NotAfter;

        if (now < notBefore)
        {
            issues.Add(new CertificateValidationIssue
            {
                Code = "CERT_NOT_YET_VALID",
                Message = $"Certificate is not valid before {notBefore:u}."
            });
        }

        if (now > notAfter)
        {
            issues.Add(new CertificateValidationIssue
            {
                Code = "CERT_EXPIRED",
                Message = $"Certificate expired at {notAfter:u}."
            });
        }
    }

    private static void ValidateRsaRequirements(X509Certificate2 certificate, List<CertificateValidationIssue> issues)
    {
        using RSA? rsa = certificate.GetRSAPublicKey();
        if (rsa is null)
        {
            issues.Add(new CertificateValidationIssue
            {
                Code = "PUBKEY_NOT_RSA",
                Message = "Certificate public key algorithm must be RSA."
            });
            return;
        }

        if (rsa.KeySize < NuGetSigningCertificateRequirements.MinimumRsaKeySizeInBits)
        {
            issues.Add(new CertificateValidationIssue
            {
                Code = "RSA_KEY_TOO_SMALL",
                Message = $"RSA key size must be at least {NuGetSigningCertificateRequirements.MinimumRsaKeySizeInBits} bits."
            });
        }
    }

    private static void ValidateKeyUsage(X509Certificate2 certificate, List<CertificateValidationIssue> issues)
    {
        X509KeyUsageExtension? keyUsage = certificate.Extensions
            .OfType<X509KeyUsageExtension>()
            .FirstOrDefault();

        if (keyUsage is null)
        {
            issues.Add(new CertificateValidationIssue
            {
                Code = "KEY_USAGE_MISSING",
                Message = "Certificate should include Key Usage with digitalSignature."
            });
            return;
        }

        if ((keyUsage.KeyUsages & X509KeyUsageFlags.DigitalSignature) == 0)
        {
            issues.Add(new CertificateValidationIssue
            {
                Code = "KEY_USAGE_DIGITAL_SIGNATURE_MISSING",
                Message = "Key Usage must include digitalSignature."
            });
        }
    }

    private static void ValidateCodeSigningEku(X509Certificate2 certificate, List<CertificateValidationIssue> issues)
    {
        X509EnhancedKeyUsageExtension? eku = certificate.Extensions
            .OfType<X509EnhancedKeyUsageExtension>()
            .FirstOrDefault();

        if (eku is null)
        {
            issues.Add(new CertificateValidationIssue
            {
                Code = "EKU_MISSING",
                Message = "Certificate should include Extended Key Usage with codeSigning."
            });
            return;
        }

        bool hasCodeSigning = eku.EnhancedKeyUsages
            .Cast<Oid>()
            .Any(static oid => oid.Value == NuGetSigningCertificateRequirements.CodeSigningEkuOid.Value);

        if (!hasCodeSigning)
        {
            issues.Add(new CertificateValidationIssue
            {
                Code = "EKU_CODE_SIGNING_MISSING",
                Message = "Extended Key Usage must include codeSigning (1.3.6.1.5.5.7.3.3)."
            });
        }
    }
}

