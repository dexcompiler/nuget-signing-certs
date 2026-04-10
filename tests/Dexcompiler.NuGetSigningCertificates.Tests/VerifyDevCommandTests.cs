using System.IO.Compression;
using Cli = Dexcompiler.NuGetSigningCertificates.Cli;

namespace Dexcompiler.NuGetSigningCertificates.Tests;

[Collection("CliCommandExecution")]
public sealed class VerifyDevCommandTests
{
    [Fact]
    public void Execute_WithSignatureEntry_ShouldSucceed()
    {
        using var tempDirectory = new TempDirectory();
        string packagePath = tempDirectory.CreateSignedPackage("signed.1.0.0.nupkg", includeSignature: true);

        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);
        try
        {
            int exitCode = Cli.VerifyDevCommand.Execute(["--input", packagePath]);

            Assert.Equal(Cli.CliExitCodes.Success, exitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    [Fact]
    public void Execute_WithoutSignatureEntry_ShouldFail()
    {
        using var tempDirectory = new TempDirectory();
        string packagePath = tempDirectory.CreateSignedPackage("unsigned.1.0.0.nupkg", includeSignature: false);

        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var output = new StringWriter();
        using var error = new StringWriter();
        Console.SetOut(output);
        Console.SetError(error);
        try
        {
            int exitCode = Cli.VerifyDevCommand.Execute(["--input", packagePath]);

            Assert.Equal(Cli.CliExitCodes.ExecutionFailure, exitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"nuget-signing-certs-tests-{Guid.NewGuid():N}");
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string CreateSignedPackage(string fileName, bool includeSignature)
        {
            string filePath = System.IO.Path.Combine(Path, fileName);

            using (FileStream stream = File.Create(filePath))
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
            {
                archive.CreateEntry("lib/net8.0/demo.dll");
                if (includeSignature)
                    archive.CreateEntry(".signature.p7s");
            }

            return filePath;
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
                Directory.Delete(Path, recursive: true);
        }
    }
}
