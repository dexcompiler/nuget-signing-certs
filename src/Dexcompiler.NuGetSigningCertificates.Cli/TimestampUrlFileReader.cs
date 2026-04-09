namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class TimestampUrlFileReader
{
    public static IReadOnlyList<string> ReadUrls(string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string fullPath = Path.GetFullPath(filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Timestamp URL file does not exist: {fullPath}");

        var urls = new List<string>();
        foreach (string rawLine in File.ReadLines(fullPath))
        {
            string line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;

            urls.Add(line);
        }

        return urls;
    }
}
