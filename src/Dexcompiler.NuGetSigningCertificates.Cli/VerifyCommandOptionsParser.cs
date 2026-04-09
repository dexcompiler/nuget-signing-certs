namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class VerifyCommandOptionsParser
{
    public static bool TryParse(string[] args, out VerifyCommandOptions? options, out string? errorMessage, out bool showHelp)
    {
        ArgumentNullException.ThrowIfNull(args);

        options = null;
        errorMessage = null;
        showHelp = false;

        var inputs = new List<string>();
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
                    if (!TryReadRequiredValue(args, ref index, argument, out string inputPath, out errorMessage))
                        return false;
                    inputs.Add(inputPath);
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

        options = new VerifyCommandOptions
        {
            Inputs = inputs,
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
