namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class Program
{
    public static int Main(string[] args)
    {
        if (args.Length == 0 || IsHelp(args[0]))
        {
            Console.WriteLine(CliHelpText.Root);
            return CliExitCodes.Success;
        }

        string command = args[0];
        string[] commandArgs = args.Skip(1).ToArray();

        return command switch
        {
            "sign" => SignCommand.Execute(commandArgs),
            "verify" => VerifyCommand.Execute(commandArgs),
            "generate-dev-cert" => GenerateDevCertificateCommand.Execute(commandArgs),
            _ => ExitWithUsageError($"Unknown command '{command}'.")
        };
    }

    private static bool IsHelp(string arg)
        => string.Equals(arg, "--help", StringComparison.OrdinalIgnoreCase)
           || string.Equals(arg, "-h", StringComparison.OrdinalIgnoreCase)
           || string.Equals(arg, "help", StringComparison.OrdinalIgnoreCase);

    private static int ExitWithUsageError(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine();
        Console.Error.WriteLine(CliHelpText.Root);
        return CliExitCodes.UsageError;
    }
}
