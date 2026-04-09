namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class SignCommandOptions
{
    public required IReadOnlyList<string> Inputs { get; init; }

    public required string PfxPath { get; init; }

    public required string PfxPassword { get; init; }

    public required string TimestampUrl { get; init; }

    public required string HashAlgorithm { get; init; }

    public required bool IncludeSnupkg { get; init; }

    public required bool Overwrite { get; init; }

    public required bool JsonOutput { get; init; }
}
