# nuget-signing-certs

`nuget-signing-certs` provides:
1. A **.NET library** for NuGet signing certificate generation/validation workflows.
2. A **CLI** (`nusign`) for signing and verifying already-packed NuGet artifacts (`.nupkg` + `.snupkg`).

## Why this exists

`nusign` exists to make NuGet package signing reliable and explicit in real-world CI/CD environments.

- Raw signing flows often fail on transient TSA/network issues; `nusign` provides built-in fallback and retry behavior.
- Teams need clear verification semantics; `nusign` separates strict trust verification from explicit dev/self-signed checks.
- Signing, verifying, and dev cert workflows are easier to automate when exposed as one consistent CLI + library surface.

## Why use `nusign` instead of raw `dotnet nuget` commands

- Built-in timestamp resilience: multi-URL fallback, retry, and timeout controls.
- Better diagnostics on timestamp failures: URL + attempt + concise reason before fallback.
- Simpler package targeting for CI: path discovery for `.nupkg` and `.snupkg`.
- Safer operator ergonomics: password via env var and redacted command display.
- Clear verification intent:
  - `verify` for strict trust-based validation
  - `verify-dev` for explicit signature-presence checks in self-signed/dev workflows.

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

## CLI quick start (`nusign`)

Install as a .NET global tool:

```bash
dotnet tool install -g nusign
```

Then run directly from your shell:

```bash
nusign --help
```

Generate local/dev signing certificate PFX:

```bash
nusign generate-dev-cert \
  --output-pfx ./artifacts/dev-signing.pfx \
  --password "<strong-password>" \
  --subject "CN=My NuGet Dev Signing Cert"
```

Sign packages from another already-packed project:

```bash
export NUGET_SIGN_CERT_PASSWORD=<strong-password>
nusign sign \
  --input ../other-project/artifacts \
  --pfx-path ./artifacts/dev-signing.pfx \
  --timestamp-url https://rfc3161.ai.moda \
  --timestamp-url https://rfc3161.ai.moda/any \
  --timestamp-url http://timestamp.digicert.com \
  --timestamp-timeout-seconds 15 \
  --timestamp-retries 2 \
  --timestamp-retry-delay-ms 500 \
  --overwrite
```

Or provide fallback URLs from a file:

```bash
nusign sign \
  --input ../other-project/artifacts \
  --pfx-path ./artifacts/dev-signing.pfx \
  --timestamp-url-file ./tsa-fallbacks.txt
```

Verify signatures:

```bash
nusign verify --input ../other-project/artifacts
```

Dev/self-signed signature presence checks (no trust-chain requirement):

```bash
nusign verify-dev --input ../other-project/artifacts
```

For machine-readable output in CI, add `--json` to `sign`, `verify`, `verify-dev`, or `generate-dev-cert`.

## Recommended fallback TSA order

1. `https://rfc3161.ai.moda`
2. `https://rfc3161.ai.moda/any`
3. `http://rfc3161.ai.moda`
4. `http://timestamp.digicert.com`
5. `http://timestamp.globalsign.com/tsa/r6advanced1`
6. `http://rfc3161timestamp.globalsign.com/advanced`
7. `http://timestamp.sectigo.com`
8. `http://time.certum.pl`
9. `http://timestamp.entrust.net/TSS/RFC3161sha2TS`
10. `http://timestamp.acs.microsoft.com`

## Verification behavior notes

- `verify` is release-safe and trust-based by default.
- `verify-dev` is explicit dev behavior (signature presence check only).
- Self-signed certificates can fail strict trust verification (`NU3018`) by design.
- Some HTTPS TSA endpoints may be blocked in specific runners/networks; fallback ordering is recommended.

## Suggested flow for external package signing

1. Generate or load a signing certificate (`.pfx`).
2. Sign the target project output artifacts (`.nupkg` and `.snupkg`) with CLI `sign`.
3. Run CLI `verify` to confirm signatures.
4. Publish signed packages.

