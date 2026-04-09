namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class VerifyDevCommandOptions
{
    public required IReadOnlyList<string> Inputs { get; init; }

    public required bool JsonOutput { get; init; }
}
