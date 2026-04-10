using Cli = Dexcompiler.NuGetSigningCertificates.Cli;

namespace Dexcompiler.NuGetSigningCertificates.Tests;

[Collection("CliCommandExecution")]
public sealed class SignCommandResilienceTests
{
    [Fact]
    public void Execute_ShouldFallbackToNextTimestampUrl_WhenFirstUrlFails()
    {
        using var tempDirectory = new TempDirectory();
        string packagePath = tempDirectory.CreateFile("demo.1.0.0.nupkg");
        string pfxPath = tempDirectory.CreateFile("cert.pfx");

        int firstUrlAttempts = 0;
        int secondUrlAttempts = 0;
        Cli.SignCommand.NuGetRunner = (arguments, _) =>
        {
            string? timestampUrl = ReadArgument(arguments, "--timestamper");
            if (timestampUrl == "https://tsa.one.example/")
            {
                firstUrlAttempts++;
                return new Cli.CommandExecutionResult
                {
                    ExitCode = 1,
                    StandardOutput = string.Empty,
                    StandardError = "No such host is known",
                    TimedOut = false
                };
            }

            secondUrlAttempts++;
            return new Cli.CommandExecutionResult
            {
                ExitCode = 0,
                StandardOutput = "signed",
                StandardError = string.Empty,
                TimedOut = false
            };
        };

        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);

        try
        {
            int exitCode = Cli.SignCommand.Execute(
            [
                "--input", packagePath,
                "--pfx-path", pfxPath,
                "--pfx-password", "pw",
                "--timestamp-url", "https://tsa.one.example",
                "--timestamp-url", "https://tsa.two.example",
                "--timestamp-retries", "0"
            ]);

            Assert.Equal(Cli.CliExitCodes.Success, exitCode);
            Assert.Equal(1, firstUrlAttempts);
            Assert.Equal(1, secondUrlAttempts);
            Assert.Contains("reason=dns", error.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("Falling back", error.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Cli.SignCommand.ResetRunner();
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    [Fact]
    public void Execute_ShouldRetrySameTimestampUrl_BeforeFallback()
    {
        using var tempDirectory = new TempDirectory();
        string packagePath = tempDirectory.CreateFile("demo.1.0.0.nupkg");
        string pfxPath = tempDirectory.CreateFile("cert.pfx");

        int attempts = 0;
        Cli.SignCommand.NuGetRunner = (_, _) =>
        {
            attempts++;
            if (attempts < 3)
            {
                return new Cli.CommandExecutionResult
                {
                    ExitCode = 1,
                    StandardOutput = string.Empty,
                    StandardError = "timed out",
                    TimedOut = true
                };
            }

            return new Cli.CommandExecutionResult
            {
                ExitCode = 0,
                StandardOutput = "signed",
                StandardError = string.Empty,
                TimedOut = false
            };
        };

        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);

        try
        {
            int exitCode = Cli.SignCommand.Execute(
            [
                "--input", packagePath,
                "--pfx-path", pfxPath,
                "--pfx-password", "pw",
                "--timestamp-url", "https://tsa.one.example",
                "--timestamp-retries", "2",
                "--timestamp-retry-delay-ms", "0"
            ]);

            Assert.Equal(Cli.CliExitCodes.Success, exitCode);
            Assert.Equal(3, attempts);
        }
        finally
        {
            Cli.SignCommand.ResetRunner();
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    [Fact]
    public void Execute_ShouldReportFailureReasons_WhenAllTimestampUrlsFail()
    {
        using var tempDirectory = new TempDirectory();
        string packagePath = tempDirectory.CreateFile("demo.1.0.0.nupkg");
        string pfxPath = tempDirectory.CreateFile("cert.pfx");

        Cli.SignCommand.NuGetRunner = (arguments, _) =>
        {
            string? timestampUrl = ReadArgument(arguments, "--timestamper");
            if (timestampUrl == "https://tsa.one.example/")
            {
                return new Cli.CommandExecutionResult
                {
                    ExitCode = 1,
                    StandardOutput = string.Empty,
                    StandardError = "request timed out",
                    TimedOut = true
                };
            }

            return new Cli.CommandExecutionResult
            {
                ExitCode = 1,
                StandardOutput = string.Empty,
                StandardError = "TLS handshake failed",
                TimedOut = false
            };
        };

        var originalError = Console.Error;
        using var error = new StringWriter();
        Console.SetError(error);
        try
        {
            int exitCode = Cli.SignCommand.Execute(
            [
                "--input", packagePath,
                "--pfx-path", pfxPath,
                "--pfx-password", "pw",
                "--timestamp-url", "https://tsa.one.example",
                "--timestamp-url", "https://tsa.two.example",
                "--timestamp-retries", "0"
            ]);

            Assert.Equal(Cli.CliExitCodes.ExecutionFailure, exitCode);
            Assert.Contains("reason=timeout", error.ToString(), StringComparison.OrdinalIgnoreCase);
            Assert.Contains("reason=tls", error.ToString(), StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Cli.SignCommand.ResetRunner();
            Console.SetError(originalError);
        }
    }

    private static string? ReadArgument(IReadOnlyList<string> arguments, string optionName)
    {
        for (int index = 0; index < arguments.Count - 1; index++)
        {
            if (string.Equals(arguments[index], optionName, StringComparison.Ordinal))
                return arguments[index + 1];
        }

        return null;
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"nuget-signing-certs-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string CreateFile(string fileName, string content = "test")
        {
            string filePath = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(filePath, content);
            return filePath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
