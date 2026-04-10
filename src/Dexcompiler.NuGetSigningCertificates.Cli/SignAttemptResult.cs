namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class SignAttemptResult
{
    public required string TimestampUrl { get; init; }

    public required int AttemptNumber { get; init; }

    public required int MaxAttemptsForUrl { get; init; }

    public required string FailureReason { get; init; }

    public required CommandExecutionResult Execution { get; init; }
}
