using System.IO.Compression;
using System.Text;
using System.Text.Json;

namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class VerifyDevCommand
{
    public static int Execute(string[] args)
    {
        if (!VerifyDevCommandOptionsParser.TryParse(args, out VerifyDevCommandOptions? options, out string? errorMessage, out bool showHelp))
            return ExitWithUsageError(errorMessage!);

        if (showHelp)
        {
            Console.WriteLine(CliHelpText.VerifyDev);
            return CliExitCodes.Success;
        }

        IReadOnlyList<string> packagePaths;
        try
        {
            packagePaths = PackageFileDiscovery.DiscoverPackages(options!.Inputs, includeSnupkg: true);
        }
        catch (Exception exception)
        {
            return ExitWithUsageError(exception.Message);
        }

        var results = new List<VerifyDevResult>(packagePaths.Count);
        foreach (string packagePath in packagePaths)
        {
            try
            {
                bool hasSignature = HasSignatureEntry(packagePath);
                results.Add(new VerifyDevResult
                {
                    PackagePath = packagePath,
                    HasSignature = hasSignature,
                    Message = hasSignature
                        ? "Package contains .signature.p7s."
                        : "Package does not contain .signature.p7s."
                });
            }
            catch (Exception exception)
            {
                results.Add(new VerifyDevResult
                {
                    PackagePath = packagePath,
                    HasSignature = false,
                    Message = $"Failed to inspect package: {exception.Message}"
                });
            }
        }

        bool success = results.All(static result => result.HasSignature);
        if (options!.JsonOutput)
            WriteJsonResult(results, success);
        else
            WriteTextResult(results, success);

        return success ? CliExitCodes.Success : CliExitCodes.ExecutionFailure;
    }

    private static bool HasSignatureEntry(string packagePath)
    {
        using ZipArchive archive = ZipFile.OpenRead(packagePath);
        return archive.Entries.Any(static entry =>
            string.Equals(entry.FullName, ".signature.p7s", StringComparison.OrdinalIgnoreCase));
    }

    private static void WriteJsonResult(IEnumerable<VerifyDevResult> results, bool success)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = true
        });

        writer.WriteStartObject();
        writer.WriteBoolean("success", success);
        writer.WritePropertyName("results");
        writer.WriteStartArray();
        foreach (VerifyDevResult result in results)
        {
            writer.WriteStartObject();
            writer.WriteString("packagePath", result.PackagePath);
            writer.WriteBoolean("hasSignature", result.HasSignature);
            writer.WriteString("message", result.Message);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static void WriteTextResult(IEnumerable<VerifyDevResult> results, bool success)
    {
        foreach (VerifyDevResult result in results)
        {
            if (result.HasSignature)
                Console.WriteLine($"[OK] {result.PackagePath} - {result.Message}");
            else
                Console.Error.WriteLine($"[FAILED] {result.PackagePath} - {result.Message}");
        }

        if (!success)
            Console.Error.WriteLine("One or more packages do not contain a signature.");
    }

    private static int ExitWithUsageError(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine();
        Console.Error.WriteLine(CliHelpText.VerifyDev);
        return CliExitCodes.UsageError;
    }

    private sealed class VerifyDevResult
    {
        public required string PackagePath { get; init; }

        public required bool HasSignature { get; init; }

        public required string Message { get; init; }
    }
}
