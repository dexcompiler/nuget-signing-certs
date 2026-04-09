namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class CliHelpText
{
    public static string Root =>
        """
        nusign

        Commands:
          sign               Sign .nupkg/.snupkg artifacts using a PFX certificate.
          verify             Verify package signatures using strict trust validation.
          verify-dev         Verify signature presence only (dev/self-signed workflows).
          generate-dev-cert  Generate a self-signed dev certificate PFX.

        Run '<command> --help' for command-specific options.
        """;

    public static string Sign =>
        """
        Usage:
          sign --input <path> [--input <path> ...] --pfx-path <file> [--pfx-password <value> | --pfx-password-env <name>] [--timestamp-url <url> ...] [--timestamp-url-file <file>] [options]

        Options:
          --input <path>             File or directory. Directories are scanned recursively.
          --pfx-path <file>          Path to certificate PFX file.
          --pfx-password <value>     Certificate password (avoid in shell history when possible).
          --pfx-password-env <name>  Environment variable for certificate password. Default: NUGET_SIGN_CERT_PASSWORD.
          --timestamp-url <url>      RFC3161 timestamp URL. Repeat to set fallback order.
          --timestamp-url-file <file>Text file with one timestamp URL per line (# comments supported).
          --timestamp-timeout-seconds Timeout per sign attempt. Default: 15.
          --timestamp-retries        Retry count per URL before fallback. Default: 2.
          --timestamp-retry-delay-ms Delay between retries per URL. Default: 500.
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
          --input <path>                  File or directory. Directories are scanned recursively.
          --certificate-fingerprint <fp>  Pass-through fingerprint filter to dotnet nuget verify.
          --configfile <file>             Pass-through NuGet config file path.
          --verbosity <level>             Pass-through verbosity (q|m|n|d|diag).
          --json                          Emit machine-readable JSON output.
          -h|--help                       Show this help.

        Notes:
          verify uses strict trust-based validation and may fail for self-signed certificates (for example NU3018) by design.
        """;

    public static string VerifyDev =>
        """
        Usage:
          verify-dev --input <path> [--input <path> ...] [options]

        Options:
          --input <path>  File or directory. Directories are scanned recursively.
          --json          Emit machine-readable JSON output.
          -h|--help       Show this help.

        Notes:
          verify-dev only checks signature presence (.signature.p7s). It does not perform trust-chain validation.
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
