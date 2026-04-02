using System.Security.Cryptography;

namespace Dexcompiler.NuGetSigningCertificates;

public static class NuGetSigningCertificateRequirements
{
    public const int MinimumRsaKeySizeInBits = 2048;

    public static readonly Oid CodeSigningEkuOid = new("1.3.6.1.5.5.7.3.3", "Code Signing");
}

