namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class GenerateDevCertificateOptionsParser
{
    private static readonly HashSet<string> AllowedHashAlgorithms = new(StringComparer.OrdinalIgnoreCase)
    {
        "SHA256",
        "SHA384",
        "SHA512"
    };

    public static bool TryParse(string[] args, out GenerateDevCertificateOptions? options, out string? errorMessage, out bool showHelp)
    {
        ArgumentNullException.ThrowIfNull(args);

        options = null;
        errorMessage = null;
        showHelp = false;

        string? outputPfxPath = null;
        string? password = null;
        string subjectName = "CN=NuGet Package Signing (Dev)";
        int validDays = 365;
        int keySizeInBits = 3072;
        string hashAlgorithm = "SHA256";
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
                case "--output-pfx":
                    if (!TryReadRequiredValue(args, ref index, argument, out outputPfxPath, out errorMessage))
                        return false;
                    break;
                case "--password":
                    if (!TryReadRequiredValue(args, ref index, argument, out password, out errorMessage))
                        return false;
                    break;
                case "--subject":
                    if (!TryReadRequiredValue(args, ref index, argument, out subjectName, out errorMessage))
                        return false;
                    break;
                case "--valid-days":
                    if (!TryReadRequiredValue(args, ref index, argument, out string validDaysText, out errorMessage))
                        return false;

                    if (!int.TryParse(validDaysText, out validDays) || validDays <= 0)
                    {
                        errorMessage = "--valid-days must be a positive integer.";
                        return false;
                    }
                    break;
                case "--key-size":
                    if (!TryReadRequiredValue(args, ref index, argument, out string keySizeText, out errorMessage))
                        return false;

                    if (!int.TryParse(keySizeText, out keySizeInBits) ||
                        keySizeInBits < NuGetSigningCertificateRequirements.MinimumRsaKeySizeInBits)
                    {
                        errorMessage = $"--key-size must be an integer >= {NuGetSigningCertificateRequirements.MinimumRsaKeySizeInBits}.";
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
                case "--json":
                    jsonOutput = true;
                    break;
                default:
                    errorMessage = $"Unknown option '{argument}'.";
                    return false;
            }
        }

        if (string.IsNullOrWhiteSpace(outputPfxPath))
        {
            errorMessage = "--output-pfx is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "--password is required.";
            return false;
        }

        if (!AllowedHashAlgorithms.Contains(hashAlgorithm))
        {
            errorMessage = $"Unsupported --hash-algorithm '{hashAlgorithm}'. Allowed: SHA256, SHA384, SHA512.";
            return false;
        }

        outputPfxPath = Path.GetFullPath(outputPfxPath);
        if (File.Exists(outputPfxPath) && !overwrite)
        {
            errorMessage = $"Output file already exists: {outputPfxPath}. Use --overwrite to replace it.";
            return false;
        }

        options = new GenerateDevCertificateOptions
        {
            OutputPfxPath = outputPfxPath,
            Password = password,
            SubjectName = subjectName,
            ValidDays = validDays,
            KeySizeInBits = keySizeInBits,
            HashAlgorithm = hashAlgorithm.ToUpperInvariant(),
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
