namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class CliHelpText
{
    public static string Root =>
        """
        Dexcompiler.NuGetSigningCertificates.Cli

        Commands:
          sign               Sign .nupkg/.snupkg artifacts using a PFX certificate.
          verify             Verify package signatures.
          generate-dev-cert  Generate a self-signed dev certificate PFX.

        Run '<command> --help' for command-specific options.
        """;

    public static string Sign =>
        """
        Usage:
          sign --input <path> [--input <path> ...] --pfx-path <file> [--pfx-password <value> | --pfx-password-env <name>] --timestamp-url <url> [options]

        Options:
          --input <path>             File or directory. Directories are scanned recursively.
          --pfx-path <file>          Path to certificate PFX file.
          --pfx-password <value>     Certificate password (avoid in shell history when possible).
          --pfx-password-env <name>  Environment variable for certificate password. Default: NUGET_SIGN_CERT_PASSWORD.
          --timestamp-url <url>      RFC3161 timestamp URL (required).
          --hash-algorithm <name>    SHA256, SHA384, or SHA512. Default: SHA256.
          --overwrite                Overwrite existing signature if already signed.
          --no-snupkg                Skip .snupkg files.
          --json                     Emit machine-readable JSON output.
          -h|--help                  Show this help.
        """;

    public static string Verify =>
        """
        Usage:
          verify --input <path> [--input <path> ...] [options]

        Options:
          --input <path>  File or directory. Directories are scanned recursively.
          --json          Emit machine-readable JSON output.
          -h|--help       Show this help.
        """;

    public static string GenerateDevCert =>
        """
        Usage:
          generate-dev-cert --output-pfx <file> --password <value> [options]

        Options:
          --output-pfx <file>      Target output path for generated PFX.
          --password <value>       PFX password.
          --subject <name>         Subject name. Default: CN=NuGet Package Signing (Dev).
          --valid-days <days>      Certificate validity in days. Default: 365.
          --key-size <bits>        RSA key size. Minimum: 2048. Default: 3072.
          --hash-algorithm <name>  SHA256, SHA384, or SHA512. Default: SHA256.
          --overwrite              Overwrite existing output file.
          --json                   Emit machine-readable JSON output.
          -h|--help                Show this help.
        """;
}
