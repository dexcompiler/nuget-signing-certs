using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal static class GenerateDevCertificateCommand
{
    public static int Execute(string[] args)
    {
        if (!GenerateDevCertificateOptionsParser.TryParse(args, out GenerateDevCertificateOptions? options, out string? errorMessage, out bool showHelp))
            return ExitWithUsageError(errorMessage!);

        if (showHelp)
        {
            Console.WriteLine(CliHelpText.GenerateDevCert);
            return CliExitCodes.Success;
        }

        var request = new CodeSigningCertificateRequest
        {
            SubjectName = options!.SubjectName,
            KeySizeInBits = options.KeySizeInBits,
            HashAlgorithm = new HashAlgorithmName(options.HashAlgorithm),
            NotBefore = DateTimeOffset.UtcNow.AddMinutes(-5),
            NotAfter = DateTimeOffset.UtcNow.AddDays(options.ValidDays)
        };

        using X509Certificate2 certificate = CodeSigningCertificateGenerator.CreateSelfSignedCertificate(request);
        CertificateValidationResult validationResult = NuGetSigningCertificateValidator.Validate(certificate);
        if (!validationResult.IsValid)
            return ExitWithValidationFailure(validationResult);

        byte[] pfx = Pkcs12CertificateStore.Export(certificate, options.Password);
        try
        {
            string? outputDirectory = Path.GetDirectoryName(options.OutputPfxPath);
            if (!string.IsNullOrEmpty(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            File.WriteAllBytes(options.OutputPfxPath, pfx);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(pfx);
        }

        if (options.JsonOutput)
            WriteJsonResult(certificate, options);
        else
            WriteTextResult(certificate, options);

        return CliExitCodes.Success;
    }

    private static int ExitWithValidationFailure(CertificateValidationResult validationResult)
    {
        Console.Error.WriteLine("Generated certificate failed NuGet validation requirements:");
        foreach (CertificateValidationIssue issue in validationResult.Issues)
            Console.Error.WriteLine($"- [{issue.Code}] {issue.Message}");

        return CliExitCodes.ExecutionFailure;
    }

    private static void WriteJsonResult(X509Certificate2 certificate, GenerateDevCertificateOptions options)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
        {
            Indented = true
        });

        writer.WriteStartObject();
        writer.WriteBoolean("success", true);
        writer.WriteString("outputPfxPath", options.OutputPfxPath);
        writer.WriteString("subject", certificate.Subject);
        writer.WriteString("thumbprint", certificate.Thumbprint);
        writer.WriteString("notBefore", certificate.NotBefore);
        writer.WriteString("notAfter", certificate.NotAfter);
        writer.WriteEndObject();
        writer.Flush();

        Console.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
    }

    private static void WriteTextResult(X509Certificate2 certificate, GenerateDevCertificateOptions options)
    {
        Console.WriteLine($"Generated certificate: {certificate.Subject}");
        Console.WriteLine($"Thumbprint: {certificate.Thumbprint}");
        Console.WriteLine($"NotAfter: {certificate.NotAfter:u}");
        Console.WriteLine($"PFX written to: {options.OutputPfxPath}");
    }

    private static int ExitWithUsageError(string message)
    {
        Console.Error.WriteLine(message);
        Console.Error.WriteLine();
        Console.Error.WriteLine(CliHelpText.GenerateDevCert);
        return CliExitCodes.UsageError;
    }
}
