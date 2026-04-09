# nuget-signing-certs

`nuget-signing-certs` provides:
1. A **.NET library** for NuGet signing certificate generation/validation workflows.
2. A **CLI** (`Dexcompiler.NuGetSigningCertificates.Cli`) for signing and verifying already-packed NuGet artifacts (`.nupkg` + `.snupkg`).

## Why this exists

This project is intentionally separate from `ed25519.cs`:

- `ed25519.cs` remains focused on Ed25519 signatures and related key/CSR helpers.
- `nuget-signing-certs` focuses on NuGet package-signing workflows that currently require RSA/X.509 code-signing profiles.

## Library features

- Generate self-signed RSA code-signing certificates with secure defaults.
- Export and import PKCS#12 (`.pfx`) certificate bundles.
- Validate certificate profile readiness for NuGet signing requirements:
  - RSA key algorithm and minimum key size.
  - Key Usage (`digitalSignature`).
  - Extended Key Usage containing `codeSigning` (`1.3.6.1.5.5.7.3.3`).
  - Certificate validity window checks.

## Library quick start

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

## CLI quick start

Build CLI:

```bash
dotnet build src/Dexcompiler.NuGetSigningCertificates.Cli/Dexcompiler.NuGetSigningCertificates.Cli.csproj -c Release
```

Generate local/dev signing certificate PFX:

```bash
dotnet run --project src/Dexcompiler.NuGetSigningCertificates.Cli -- generate-dev-cert \
  --output-pfx ./artifacts/dev-signing.pfx \
  --password "<strong-password>" \
  --subject "CN=My NuGet Dev Signing Cert"
```

Sign packages from another already-packed project:

```bash
export NUGET_SIGN_CERT_PASSWORD=<strong-password>
dotnet run --project src/Dexcompiler.NuGetSigningCertificates.Cli -- sign \
  --input ../other-project/artifacts \
  --pfx-path ./artifacts/dev-signing.pfx \
  --timestamp-url https://timestamp.digicert.com \
  --overwrite
```

Verify signatures:

```bash
dotnet run --project src/Dexcompiler.NuGetSigningCertificates.Cli -- verify \
  --input ../other-project/artifacts
```

For machine-readable output in CI, add `--json` to `sign`, `verify`, or `generate-dev-cert`.

## Suggested flow for external package signing

1. Generate or load a signing certificate (`.pfx`).
2. Sign the target project output artifacts (`.nupkg` and `.snupkg`) with CLI `sign`.
3. Run CLI `verify` to confirm signatures.
4. Publish signed packages.

