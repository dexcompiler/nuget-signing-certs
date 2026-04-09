namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class PackageCommandResult
{
    public required string PackagePath { get; init; }

    public required string DisplayCommand { get; init; }

    public required CommandExecutionResult Execution { get; init; }

    public IReadOnlyList<SignAttemptResult>? FailedAttempts { get; init; }
}
