using System.Diagnostics;

namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class DotNetNuGetRunner
{
    public static CommandExecutionResult Run(IReadOnlyList<string> arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);

        var startInfo = new ProcessStartInfo("dotnet")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using Process? process = Process.Start(startInfo);
        if (process is null)
            throw new InvalidOperationException("Failed to start dotnet process.");

        Task<string> stdOutTask = process.StandardOutput.ReadToEndAsync();
        Task<string> stdErrTask = process.StandardError.ReadToEndAsync();
        Task exitTask = process.WaitForExitAsync();

        Task.WaitAll(stdOutTask, stdErrTask, exitTask);

        return new CommandExecutionResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = stdOutTask.Result,
            StandardError = stdErrTask.Result
        };
    }

    public static string FormatCommand(IReadOnlyList<string> arguments, params int[] redactedArgumentIndexes)
    {
        var redacted = new HashSet<int>(redactedArgumentIndexes);
        IEnumerable<string> formattedArguments = arguments
            .Select((arg, index) => redacted.Contains(index) ? "***" : arg)
            .Select(Quote);

        return $"dotnet {string.Join(' ', formattedArguments)}";
    }

    private static string Quote(string value)
    {
        if (value.Length == 0)
            return "\"\"";

        if (value.IndexOfAny([' ', '\t', '"']) < 0)
            return value;

        return $"\"{value.Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
    }
}
