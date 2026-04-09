namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class VerifyCommandOptions
{
    public required IReadOnlyList<string> Inputs { get; init; }

    public required bool JsonOutput { get; init; }
}
