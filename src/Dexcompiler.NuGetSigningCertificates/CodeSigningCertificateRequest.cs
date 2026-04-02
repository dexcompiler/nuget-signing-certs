using System.Security.Cryptography;

namespace Dexcompiler.NuGetSigningCertificates;

public sealed class CodeSigningCertificateRequest
{
    public string SubjectName { get; init; } = "CN=NuGet Package Signing";

    public int KeySizeInBits { get; init; } = 3072;

    public HashAlgorithmName HashAlgorithm { get; init; } = HashAlgorithmName.SHA256;

    public DateTimeOffset NotBefore { get; init; } = DateTimeOffset.UtcNow.AddMinutes(-5);

    public DateTimeOffset NotAfter { get; init; } = DateTimeOffset.UtcNow.AddYears(1);
}

