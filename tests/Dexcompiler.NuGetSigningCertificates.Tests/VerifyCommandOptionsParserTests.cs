using Cli = Dexcompiler.NuGetSigningCertificates.Cli;

namespace Dexcompiler.NuGetSigningCertificates.Tests;

public sealed class VerifyCommandOptionsParserTests
{
    [Fact]
    public void TryParse_WithPassThroughOptions_ShouldSucceed()
    {
        string[] args =
        [
            "--input", "pkg.nupkg",
            "--certificate-fingerprint", "ABC123",
            "--certificate-fingerprint", "DEF456",
            "--configfile", "nuget.config",
            "--verbosity", "detailed",
            "--json"
        ];

        bool parsed = Cli.VerifyCommandOptionsParser.TryParse(args, out Cli.VerifyCommandOptions? options, out string? error, out bool showHelp);

        Assert.True(parsed);
        Assert.False(showHelp);
        Assert.Null(error);
        Assert.NotNull(options);
        Assert.Equal(["ABC123", "DEF456"], options.CertificateFingerprints);
        Assert.Equal("nuget.config", options.ConfigFile);
        Assert.Equal("detailed", options.Verbosity);
        Assert.True(options.JsonOutput);
    }
}
