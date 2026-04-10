namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class TimestampFailureReasonClassifier
{
    public static string Classify(CommandExecutionResult execution)
    {
        ArgumentNullException.ThrowIfNull(execution);

        if (execution.TimedOut)
            return "timeout";

        string text = $"{execution.StandardError}\n{execution.StandardOutput}".ToLowerInvariant();
        if (ContainsAny(text, "no such host", "name or service not known", "host not found", "dns"))
            return "dns";
        if (ContainsAny(text, "tls", "ssl", "handshake", "remote certificate", "authentication failed"))
            return "tls";
        if (ContainsAny(text, "timestamp", "timestamper", "rfc 3161", "rfc3161", "tsa"))
            return "tsa response";

        return "other";
    }

    private static bool ContainsAny(string text, params string[] values)
        => values.Any(text.Contains);
}
