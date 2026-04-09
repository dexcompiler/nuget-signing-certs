namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class SignCommandOptionsParser
{
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
        string? timestampUrl = null;
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
                    if (!TryReadRequiredValue(args, ref index, argument, out timestampUrl, out errorMessage))
                        return false;
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

        if (string.IsNullOrWhiteSpace(timestampUrl))
        {
            errorMessage = "--timestamp-url is required.";
            return false;
        }

        if (!Uri.TryCreate(timestampUrl, UriKind.Absolute, out Uri? parsedTimestampUrl) ||
            (parsedTimestampUrl.Scheme != Uri.UriSchemeHttp && parsedTimestampUrl.Scheme != Uri.UriSchemeHttps))
        {
            errorMessage = "--timestamp-url must be an absolute http/https URL.";
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
            TimestampUrl = timestampUrl,
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
        if (valueIndex >= args.Length || args[valueIndex].StartsWith("-", StringComparison.Ordinal))
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
}
