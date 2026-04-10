namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class SignCommandOptions
{
    public required IReadOnlyList<string> Inputs { get; init; }

    public required string PfxPath { get; init; }

    public required string PfxPassword { get; init; }

    public required IReadOnlyList<string> TimestampUrls { get; init; }

    public required int TimestampTimeoutSeconds { get; init; }

    public required int TimestampRetries { get; init; }

    public required int TimestampRetryDelayMilliseconds { get; init; }

    public required string HashAlgorithm { get; init; }

    public required bool IncludeSnupkg { get; init; }

    public required bool Overwrite { get; init; }

    public required bool JsonOutput { get; init; }
}
