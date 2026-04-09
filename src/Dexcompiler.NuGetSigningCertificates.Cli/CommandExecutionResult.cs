namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class CommandExecutionResult
{
    public required int ExitCode { get; init; }

    public required string StandardOutput { get; init; }

    public required string StandardError { get; init; }
}
