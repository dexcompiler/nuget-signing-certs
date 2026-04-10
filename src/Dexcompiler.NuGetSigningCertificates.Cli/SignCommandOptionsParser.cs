namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class SignCommandOptionsParser
{
    private const int DefaultTimestampTimeoutSeconds = 15;
    private const int DefaultTimestampRetries = 2;
    private const int DefaultTimestampRetryDelayMilliseconds = 500;

    private static readonly HashSet<string> AllowedHashAlgorithms = new(StringComparer.OrdinalIgnoreCase)
    {
        "SHA256",
        "SHA384",
        "SHA512"
    };

    public static bool TryParse(string[] args, out SignCommandOptions? options, out string? errorMessage, out bool showHelp)
    {
        ArgumentNullException.ThrowIfNull(args);

        options = null;
        errorMessage = null;
        showHelp = false;

        var inputs = new List<string>();
        string? pfxPath = null;
        string? pfxPassword = null;
        string pfxPasswordEnvName = "NUGET_SIGN_CERT_PASSWORD";
        var timestampUrlsFromArgs = new List<string>();
        string? timestampUrlFilePath = null;
        int timestampTimeoutSeconds = DefaultTimestampTimeoutSeconds;
        int timestampRetries = DefaultTimestampRetries;
        int timestampRetryDelayMilliseconds = DefaultTimestampRetryDelayMilliseconds;
        string hashAlgorithm = "SHA256";
        bool includeSnupkg = true;
        bool overwrite = false;
        bool jsonOutput = false;

        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index];
            switch (argument)
            {
                case "--help":
                case "-h":
                    showHelp = true;
                    return true;
                case "--input":
                    if (!TryReadRequiredValue(args, ref index, argument, out string? inputValue, out errorMessage))
                        return false;
                    inputs.Add(inputValue);
                    break;
                case "--pfx-path":
                    if (!TryReadRequiredValue(args, ref index, argument, out pfxPath, out errorMessage))
                        return false;
                    break;
                case "--pfx-password":
                    if (!TryReadRequiredValue(args, ref index, argument, out pfxPassword, out errorMessage))
                        return false;
                    break;
                case "--pfx-password-env":
                    if (!TryReadRequiredValue(args, ref index, argument, out pfxPasswordEnvName, out errorMessage))
                        return false;
                    break;
                case "--timestamp-url":
                    if (!TryReadRequiredValue(args, ref index, argument, out string timestampUrl, out errorMessage))
                        return false;
                    timestampUrlsFromArgs.Add(timestampUrl);
                    break;
                case "--timestamp-url-file":
                    if (!TryReadRequiredValue(args, ref index, argument, out timestampUrlFilePath, out errorMessage))
                        return false;
                    break;
                case "--timestamp-timeout-seconds":
                    if (!TryReadRequiredValue(args, ref index, argument, out string timeoutText, out errorMessage))
                        return false;
                    if (!int.TryParse(timeoutText, out timestampTimeoutSeconds) || timestampTimeoutSeconds < 1 || timestampTimeoutSeconds > 300)
                    {
                        errorMessage = "--timestamp-timeout-seconds must be an integer between 1 and 300.";
                        return false;
                    }
                    break;
                case "--timestamp-retries":
                    if (!TryReadRequiredValue(args, ref index, argument, out string retriesText, out errorMessage))
                        return false;
                    if (!int.TryParse(retriesText, out timestampRetries) || timestampRetries < 0 || timestampRetries > 10)
                    {
                        errorMessage = "--timestamp-retries must be an integer between 0 and 10.";
                        return false;
                    }
                    break;
                case "--timestamp-retry-delay-ms":
                    if (!TryReadRequiredValue(args, ref index, argument, out string delayText, out errorMessage))
                        return false;
                    if (!int.TryParse(delayText, out timestampRetryDelayMilliseconds) || timestampRetryDelayMilliseconds < 0 || timestampRetryDelayMilliseconds > 60000)
                    {
                        errorMessage = "--timestamp-retry-delay-ms must be an integer between 0 and 60000.";
                        return false;
                    }
                    break;
                case "--hash-algorithm":
                    if (!TryReadRequiredValue(args, ref index, argument, out hashAlgorithm, out errorMessage))
                        return false;
                    break;
                case "--overwrite":
                    overwrite = true;
                    break;
                case "--no-snupkg":
                    includeSnupkg = false;
                    break;
                case "--json":
                    jsonOutput = true;
                    break;
                default:
                    errorMessage = $"Unknown option '{argument}'.";
                    return false;
            }
        }

        if (inputs.Count == 0)
        {
            errorMessage = "At least one --input path is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(pfxPath))
        {
            errorMessage = "--pfx-path is required.";
            return false;
        }

        pfxPath = Path.GetFullPath(pfxPath);
        if (!File.Exists(pfxPath))
        {
            errorMessage = $"PFX file does not exist: {pfxPath}";
            return false;
        }

        IReadOnlyList<string> timestampUrls;
        try
        {
            timestampUrls = BuildTimestampUrls(timestampUrlsFromArgs, timestampUrlFilePath);
        }
        catch (Exception exception)
        {
            errorMessage = exception.Message;
            return false;
        }

        if (!AllowedHashAlgorithms.Contains(hashAlgorithm))
        {
            errorMessage = $"Unsupported --hash-algorithm '{hashAlgorithm}'. Allowed: SHA256, SHA384, SHA512.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(pfxPassword))
            pfxPassword = Environment.GetEnvironmentVariable(pfxPasswordEnvName);

        if (string.IsNullOrWhiteSpace(pfxPassword))
        {
            errorMessage = $"PFX password is required via --pfx-password or environment variable '{pfxPasswordEnvName}'.";
            return false;
        }

        options = new SignCommandOptions
        {
            Inputs = inputs,
            PfxPath = pfxPath,
            PfxPassword = pfxPassword,
            TimestampUrls = timestampUrls,
            TimestampTimeoutSeconds = timestampTimeoutSeconds,
            TimestampRetries = timestampRetries,
            TimestampRetryDelayMilliseconds = timestampRetryDelayMilliseconds,
            HashAlgorithm = hashAlgorithm.ToUpperInvariant(),
            IncludeSnupkg = includeSnupkg,
            Overwrite = overwrite,
            JsonOutput = jsonOutput
        };

        return true;
    }

    private static bool TryReadRequiredValue(
        string[] args,
        ref int index,
        string optionName,
        out string value,
        out string? errorMessage)
    {
        int valueIndex = index + 1;
        if (valueIndex >= args.Length || args[valueIndex].StartsWith('-'))
        {
            value = string.Empty;
            errorMessage = $"Missing value for {optionName}.";
            return false;
        }

        value = args[valueIndex];
        index = valueIndex;
        errorMessage = null;
        return true;
    }

    private static IReadOnlyList<string> BuildTimestampUrls(IReadOnlyList<string> cliUrls, string? timestampUrlFilePath)
    {
        var orderedUrls = new List<string>();
        var dedup = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string cliUrl in cliUrls)
            AddTimestampUrl(orderedUrls, dedup, cliUrl);

        if (!string.IsNullOrWhiteSpace(timestampUrlFilePath))
        {
            foreach (string fileUrl in TimestampUrlFileReader.ReadUrls(timestampUrlFilePath))
                AddTimestampUrl(orderedUrls, dedup, fileUrl);
        }

        if (orderedUrls.Count == 0)
            throw new InvalidOperationException("At least one timestamp URL is required via --timestamp-url or --timestamp-url-file.");

        return orderedUrls;
    }

    private static void AddTimestampUrl(List<string> orderedUrls, HashSet<string> dedup, string candidateUrl)
    {
        if (string.IsNullOrWhiteSpace(candidateUrl))
            throw new InvalidOperationException("Timestamp URL values must not be empty.");

        if (!Uri.TryCreate(candidateUrl, UriKind.Absolute, out Uri? parsedTimestampUrl) ||
            (parsedTimestampUrl.Scheme != Uri.UriSchemeHttp && parsedTimestampUrl.Scheme != Uri.UriSchemeHttps))
        {
            throw new InvalidOperationException($"Timestamp URL must be absolute http/https: {candidateUrl}");
        }

        string normalized = parsedTimestampUrl.AbsoluteUri;
        if (dedup.Add(normalized))
            orderedUrls.Add(normalized);
    }
}
