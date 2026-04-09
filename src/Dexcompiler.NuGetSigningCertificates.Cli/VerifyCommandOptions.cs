namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class VerifyCommandOptions
{
    public required IReadOnlyList<string> Inputs { get; init; }

    public required IReadOnlyList<string> CertificateFingerprints { get; init; }

    public string? ConfigFile { get; init; }

    public string? Verbosity { get; init; }

    public required bool JsonOutput { get; init; }
}
