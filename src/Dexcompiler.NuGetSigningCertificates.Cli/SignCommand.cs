using System.Text;
using System.Text.Json;

namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class SignCommand
{
    public static int Execute(string[] args)
    {
        if (!SignCommandOptionsParser.TryParse(args, out SignCommandOptions? options, out string? errorMessage, out bool showHelp))
            return ExitWithUsageError(errorMessage!);

        if (showHelp)
        {
            Console.WriteLine(CliHelpText.Sign);
            return CliExitCodes.Success;
        }

        IReadOnlyList<string> packagePaths;
        try
        {
            packagePaths = PackageFileDiscovery.DiscoverPackages(options!.Inputs, options.IncludeSnupkg);
        }
        catch (Exception exception)
        {
            return ExitWithUsageError(exception.Message);
        }

        var results = new List<PackageCommandResult>(packagePaths.Count);

        foreach (string packagePath in packagePaths)
        {
            List<string> commandArguments = BuildSignArguments(packagePath, options!);
            int passwordArgumentIndex = FindPasswordArgumentIndex(commandArguments);

            CommandExecutionResult execution = DotNetNuGetRunner.Run(commandArguments);
            results.Add(new PackageCommandResult
            {
                PackagePath = packagePath,
                DisplayCommand = passwordArgumentIndex >= 0
                    ? DotNetNuGetRunner.FormatCommand(commandArguments, passwordArgumentIndex)
                    : DotNetNuGetRunner.FormatCommand(commandArguments),
                Execution = execution
            });
        }

        bool success = results.All(static result => result.Execution.ExitCode == 0);
        if (options!.JsonOutput)
            WriteJsonResult(results, success);
        else
            WriteTextResult(results, success);

        return success ? CliExitCodes.Success : CliExitCodes.ExecutionFailure;
    }

    private static List<string> BuildSignArguments(string packagePath, SignCommandOptions options)
    {
        var commandArguments = new List<string>
        {
            "nuget",
            "sign",
            packagePath,
            "--certificate-path",
            options.PfxPath,
            "--certificate-password",
            options.PfxPassword,
            "--timestamper",
            options.TimestampUrl,
            "--hash-algorithm",
            options.HashAlgorithm
        };

        if (options.Overwrite)
            commandArguments.Add("--overwrite");

        return commandArguments;
    }

    private static int FindPasswordArgumentIndex(IReadOnlyList<string> commandArguments)
    {
        for (int index = 0; index < commandArguments.Count - 1; index++)
        {
            if (string.Equals(commandArguments[index], "--certificate-password", StringComparison.Ordinal))
                return index + 1;
        }

        return -1;
    }

    private static void WriteJsonResult(IEnumerable<PackageCommandResult> results, bool success)
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

        foreach (PackageCommandResult result in results)
        {
            writer.WriteStartObject();
            writer.WriteString("packagePath", result.PackagePath);
            writer.WriteString("command", result.DisplayCommand);
            writer.WriteNumber("exitCode", result.Execution.ExitCode);
            writer.WriteString("standardOutput", result.Execution.StandardOutput);
            writer.WriteString("standardError", result.Execution.StandardError);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
        writer.Flush();

        Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static void WriteTextResult(IEnumerable<PackageCommandResult> results, bool success)
    {
        foreach (PackageCommandResult result in results)
        {
            if (result.Execution.ExitCode == 0)
            {
                Console.WriteLine($"[OK] {result.PackagePath}");
                continue;
            }

            Console.Error.WriteLine($"[FAILED] {result.PackagePath}");
            Console.Error.WriteLine(result.DisplayCommand);

            if (!string.IsNullOrWhiteSpace(result.Execution.StandardError))
                Console.Error.WriteLine(result.Execution.StandardError.Trim());

            if (!string.IsNullOrWhiteSpace(result.Execution.StandardOutput))
                Console.Error.WriteLine(result.Execution.StandardOutput.Trim());
        }

        if (!success)
            Console.Error.WriteLine("One or more package signing commands failed.");
    }

    private static int ExitWithUsageError(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine();
        Console.Error.WriteLine(CliHelpText.Sign);
        return CliExitCodes.UsageError;
    }
}
