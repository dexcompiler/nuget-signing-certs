namespace Dexcompiler.NuGetSigningCertificates;

public sealed class CertificateValidationResult
{
    public required IReadOnlyList<CertificateValidationIssue> Issues { get; init; }

    public bool IsValid => Issues.Count == 0;
}

