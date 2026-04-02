# nuget-signing-certs

`nuget-signing-certs` is a .NET companion library for generating NuGet-compatible RSA code-signing certificates, exporting PFX artifacts, and validating certificate profiles before signing packages.

## Why this exists

This project is intentionally separate from `ed25519.cs`:

- `ed25519.cs` remains focused on Ed25519 signatures and related key/CSR helpers.
- `nuget-signing-certs` focuses on NuGet package-signing certificate workflows that currently require RSA/X.509 code-signing profiles.

## Features

- Generate self-signed RSA code-signing certificates with secure defaults.
- Export and import PKCS#12 (`.pfx`) certificate bundles.
- Validate certificate profile readiness for NuGet signing requirements:
  - RSA key algorithm and minimum key size.
  - Key Usage (`digitalSignature`).
  - Extended Key Usage containing `codeSigning` (`1.3.6.1.5.5.7.3.3`).
  - Certificate validity window checks.

## Quick start

```csharp
using Dexcompiler.NuGetSigningCertificates;

var cert = CodeSigningCertificateGenerator.CreateSelfSignedCertificate(
    new CodeSigningCertificateRequest
    {
        SubjectName = "CN=My NuGet Signing Cert",
        KeySizeInBits = 3072
    });

var validation = NuGetSigningCertificateValidator.Validate(cert);
if (!validation.IsValid)
{
    throw new InvalidOperationException(string.Join(Environment.NewLine, validation.Issues.Select(i => i.Message)));
}

byte[] pfx = Pkcs12CertificateStore.Export(cert, "strong-password");
```

## Suggested signing flow

1. Generate or load your signing certificate.
2. Validate with `NuGetSigningCertificateValidator`.
3. Export `.pfx`.
4. Use `dotnet nuget sign` with your certificate/PFX.

