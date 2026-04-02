using System.Security.Cryptography.X509Certificates;

namespace Dexcompiler.NuGetSigningCertificates.Tests;

public sealed class Pkcs12CertificateStoreTests
{
    [Fact]
    public void ExportAndImport_RoundTrip_ShouldPreserveCertificateIdentity()
    {
        using X509Certificate2 cert = CodeSigningCertificateGenerator.CreateSelfSignedCertificate(
            new CodeSigningCertificateRequest
            {
                SubjectName = "CN=pfx-roundtrip"
            });

        const string password = "pfx-test-password-123";

        byte[] pfx = Pkcs12CertificateStore.Export(cert, password);
        using X509Certificate2 imported = Pkcs12CertificateStore.Import(pfx, password);

        Assert.True(imported.HasPrivateKey);
        Assert.Equal(cert.Thumbprint, imported.Thumbprint);
    }
}

