namespace Dexcompiler.NuGetSigningCertificates;

public sealed class CertificateValidationIssue
{
    public required string Code { get; init; }

    public required string Message { get; init; }
}

