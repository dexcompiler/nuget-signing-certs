using Cli = Dexcompiler.NuGetSigningCertificates.Cli;

namespace Dexcompiler.NuGetSigningCertificates.Tests;

public sealed class SignCommandOptionsParserTests
{
    [Fact]
    public void TryParse_WithValidArgumentsAndEnvPassword_ShouldSucceed()
    {
        using var tempDirectory = new TempDirectory();
        string pfxPath = tempDirectory.CreateFile("cert.pfx");
        string packagePath = tempDirectory.CreateFile("demo.1.0.0.nupkg");

        const string passwordEnvName = "NUGET_SIGNING_CERT_PASSWORD_TEST";
        string? previousValue = Environment.GetEnvironmentVariable(passwordEnvName);
        Environment.SetEnvironmentVariable(passwordEnvName, "test-password");

        try
        {
            string[] args =
            [
                "--input", packagePath,
                "--pfx-path", pfxPath,
                "--pfx-password-env", passwordEnvName,
                "--timestamp-url", "https://timestamp.example.com"
            ];

            bool parsed = Cli.SignCommandOptionsParser.TryParse(args, out Cli.SignCommandOptions? options, out string? error, out bool showHelp);

            Assert.True(parsed);
            Assert.False(showHelp);
            Assert.Null(error);
            Assert.NotNull(options);
            Assert.Equal("test-password", options.PfxPassword);
            Assert.Equal("SHA256", options.HashAlgorithm);
        }
        finally
        {
            Environment.SetEnvironmentVariable(passwordEnvName, previousValue);
        }
    }

    [Fact]
    public void TryParse_MissingTimestamp_ShouldFail()
    {
        using var tempDirectory = new TempDirectory();
        string pfxPath = tempDirectory.CreateFile("cert.pfx");
        string packagePath = tempDirectory.CreateFile("demo.1.0.0.nupkg");

        string[] args =
        [
            "--input", packagePath,
            "--pfx-path", pfxPath,
            "--pfx-password", "test-password"
        ];

        bool parsed = Cli.SignCommandOptionsParser.TryParse(args, out _, out string? error, out bool showHelp);

        Assert.False(parsed);
        Assert.False(showHelp);
        Assert.Contains("--timestamp-url is required", error, StringComparison.Ordinal);
    }

    [Fact]
    public void TryParse_UnknownOption_ShouldFail()
    {
        using var tempDirectory = new TempDirectory();
        string pfxPath = tempDirectory.CreateFile("cert.pfx");
        string packagePath = tempDirectory.CreateFile("demo.1.0.0.nupkg");

        string[] args =
        [
            "--input", packagePath,
            "--pfx-path", pfxPath,
            "--pfx-password", "test-password",
            "--timestamp-url", "https://timestamp.example.com",
            "--wat"
        ];

        bool parsed = Cli.SignCommandOptionsParser.TryParse(args, out _, out string? error, out bool showHelp);

        Assert.False(parsed);
        Assert.False(showHelp);
        Assert.Contains("Unknown option", error, StringComparison.Ordinal);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"nuget-signing-certs-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string CreateFile(string fileName)
        {
            string filePath = System.IO.Path.Combine(Path, fileName);
            File.WriteAllText(filePath, "test");
            return filePath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
