namespace Dexcompiler.NuGetSigningCertificates.Cli;

internal sealed class GenerateDevCertificateOptions
{
    public required string OutputPfxPath { get; init; }

    public required string Password { get; init; }

    public required string SubjectName { get; init; }

    public required int ValidDays { get; init; }

    public required int KeySizeInBits { get; init; }

    public required string HashAlgorithm { get; init; }

    public required bool Overwrite { get; init; }

    public required bool JsonOutput { get; init; }
}
