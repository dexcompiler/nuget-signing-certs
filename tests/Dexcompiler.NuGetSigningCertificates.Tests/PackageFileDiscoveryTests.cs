using Cli = Dexcompiler.NuGetSigningCertificates.Cli;

namespace Dexcompiler.NuGetSigningCertificates.Tests;

public sealed class PackageFileDiscoveryTests
{
    [Fact]
    public void DiscoverPackages_WithDirectoryInput_ShouldIncludeNupkgAndSnupkg()
    {
        using var tempDirectory = new TempDirectory();
        string nupkgPath = tempDirectory.CreateFile("demo.1.0.0.nupkg");
        string snupkgPath = tempDirectory.CreateFile("demo.1.0.0.snupkg");

        IReadOnlyList<string> discovered = Cli.PackageFileDiscovery.DiscoverPackages([tempDirectory.Path], includeSnupkg: true);

        Assert.Contains(Path.GetFullPath(nupkgPath), discovered);
        Assert.Contains(Path.GetFullPath(snupkgPath), discovered);
    }

    [Fact]
    public void DiscoverPackages_WithNoSnupkgFlag_ShouldExcludeSnupkg()
    {
        using var tempDirectory = new TempDirectory();
        string nupkgPath = tempDirectory.CreateFile("demo.1.0.0.nupkg");
        tempDirectory.CreateFile("demo.1.0.0.snupkg");

        IReadOnlyList<string> discovered = Cli.PackageFileDiscovery.DiscoverPackages([tempDirectory.Path], includeSnupkg: false);

        Assert.Single(discovered);
        Assert.Equal(Path.GetFullPath(nupkgPath), discovered[0]);
    }

    [Fact]
    public void DiscoverPackages_WithUnsupportedExtension_ShouldThrow()
    {
        using var tempDirectory = new TempDirectory();
        string txtPath = tempDirectory.CreateFile("notes.txt");

        InvalidOperationException exception = Assert.Throws<InvalidOperationException>(
            () => Cli.PackageFileDiscovery.DiscoverPackages([txtPath], includeSnupkg: true));

        Assert.Contains("Unsupported package extension", exception.Message, StringComparison.Ordinal);
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
