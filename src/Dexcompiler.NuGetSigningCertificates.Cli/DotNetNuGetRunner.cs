using System.Diagnostics;

namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class DotNetNuGetRunner
{
    public static CommandExecutionResult Run(IReadOnlyList<string> arguments, TimeSpan? timeout = null)
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
        Task completion = Task.WhenAll(stdOutTask, stdErrTask, exitTask);

        bool timedOut = false;
        if (timeout.HasValue)
        {
            bool completedInTime = completion.Wait(timeout.Value);
            if (!completedInTime)
            {
                timedOut = true;
                TryTerminateProcess(process);
                completion.Wait();
            }
        }
        else
        {
            completion.Wait();
        }

        return new CommandExecutionResult
        {
            ExitCode = process.ExitCode,
            StandardOutput = stdOutTask.Result,
            StandardError = stdErrTask.Result,
            TimedOut = timedOut
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

    private static void TryTerminateProcess(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // Best effort terminate on timeout; preserve original command output for diagnostics.
        }
    }
}
