namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class PackageFileDiscovery
{
    private static readonly StringComparer PathComparer = OperatingSystem.IsWindows()
        ? StringComparer.OrdinalIgnoreCase
        : StringComparer.Ordinal;

    public static IReadOnlyList<string> DiscoverPackages(IEnumerable<string> inputs, bool includeSnupkg)
    {
        ArgumentNullException.ThrowIfNull(inputs);

        var discovered = new HashSet<string>(PathComparer);

        foreach (string input in inputs)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input path must not be empty.", nameof(inputs));

            string fullPath = Path.GetFullPath(input);
            if (File.Exists(fullPath))
            {
                AddFileIfSupported(fullPath, includeSnupkg, discovered);
                continue;
            }

            if (Directory.Exists(fullPath))
            {
                AddFromDirectory(fullPath, includeSnupkg, discovered);
                continue;
            }

            throw new FileNotFoundException($"Input path does not exist: {fullPath}");
        }

        if (discovered.Count == 0)
            throw new InvalidOperationException("No package files were discovered from the supplied --input values.");

        return discovered
            .OrderBy(static path => path, PathComparer)
            .ToArray();
    }

    private static void AddFromDirectory(string directoryPath, bool includeSnupkg, HashSet<string> discovered)
    {
        foreach (string packagePath in Directory.EnumerateFiles(directoryPath, "*.nupkg", SearchOption.AllDirectories))
            discovered.Add(Path.GetFullPath(packagePath));

        if (!includeSnupkg)
            return;

        foreach (string packagePath in Directory.EnumerateFiles(directoryPath, "*.snupkg", SearchOption.AllDirectories))
            discovered.Add(Path.GetFullPath(packagePath));
    }

    private static void AddFileIfSupported(string packagePath, bool includeSnupkg, HashSet<string> discovered)
    {
        string extension = Path.GetExtension(packagePath);
        if (string.Equals(extension, ".nupkg", StringComparison.OrdinalIgnoreCase))
        {
            discovered.Add(packagePath);
            return;
        }

        if (string.Equals(extension, ".snupkg", StringComparison.OrdinalIgnoreCase))
        {
            if (!includeSnupkg)
                throw new InvalidOperationException("Input includes .snupkg while --no-snupkg is specified.");

            discovered.Add(packagePath);
            return;
        }

        throw new InvalidOperationException($"Unsupported package extension for path: {packagePath}");
    }
}
