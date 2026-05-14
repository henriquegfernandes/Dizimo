# Create self-signed certificates for code signing
# Run this script ONCE to generate certificates
# The generated certificates will be stored in the certs/ directory

param(
    [string]$CertDir = "C:\Projects\Dizimo\certs",
    [string]$CertPassword = "DizimoDev2024!",
    [string]$CertCommonName = "Henrique Fernandes Tech (Development)"
)

Write-Host "🔐 Creating self-signed certificates for code signing..." -ForegroundColor Cyan

# Create certs directory if it doesn't exist
if (-not (Test-Path $CertDir)) {
    New-Item -ItemType Directory -Path $CertDir -Force | Out-Null
    Write-Host "📁 Created directory: $CertDir"
}

# ============================================================================
# Windows Code Signing Certificate (PFX)
# ============================================================================
Write-Host "`n📝 Creating Windows Code Signing Certificate..." -ForegroundColor Yellow

$WindowsCertPath = Join-Path $CertDir "dizimo-codesign.pfx"

if (Test-Path $WindowsCertPath) {
    Write-Host "⚠️  Windows certificate already exists: $WindowsCertPath"
} else {
    # Create self-signed certificate
    $cert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject "CN=$CertCommonName,O=Henrique Fernandes Tech,C=BR" `
        -KeyUsage DigitalSignature `
        -KeySpec Signature `
        -KeyLength 2048 `
        -CertStoreLocation Cert:\CurrentUser\My `
        -NotAfter (Get-Date).AddYears(5) `
        -FriendlyName "Dizimo Code Signing (Self-Signed)"

    Write-Host "✅ Certificate created: $($cert.Thumbprint)"

    # Export to PFX with password
    $securePassword = ConvertTo-SecureString -String $CertPassword -AsPlainText -Force
    Export-PfxCertificate `
        -Cert $cert `
        -FilePath $WindowsCertPath `
        -Password $securePassword `
        -Force | Out-Null

    Write-Host "✅ Windows certificate exported to: $WindowsCertPath"
    Write-Host "   ⚠️  Password: $CertPassword (store securely)"
}

# ============================================================================
# macOS Code Signing Certificate (also PFX, but formatted for macOS)
# ============================================================================
Write-Host "`n📝 Creating macOS Code Signing Certificate..." -ForegroundColor Yellow

$macOSCertPath = Join-Path $CertDir "dizimo-macos-codesign.pfx"

if (Test-Path $macOSCertPath) {
    Write-Host "⚠️  macOS certificate already exists: $macOSCertPath"
} else {
    # Create self-signed certificate for macOS
    $macCert = New-SelfSignedCertificate `
        -Type CodeSigningCert `
        -Subject "CN=$CertCommonName,O=Henrique Fernandes Tech,C=BR" `
        -KeyUsage DigitalSignature `
        -KeySpec Signature `
        -KeyLength 2048 `
        -CertStoreLocation Cert:\CurrentUser\My `
        -NotAfter (Get-Date).AddYears(5) `
        -FriendlyName "Dizimo macOS Code Signing (Self-Signed)"

    Write-Host "✅ Certificate created: $($macCert.Thumbprint)"

    # Export to PFX
    $securePassword = ConvertTo-SecureString -String $CertPassword -AsPlainText -Force
    Export-PfxCertificate `
        -Cert $macCert `
        -FilePath $macOSCertPath `
        -Password $securePassword `
        -Force | Out-Null

    Write-Host "✅ macOS certificate exported to: $macOSCertPath"
}

# ============================================================================
# Summary
# ============================================================================
Write-Host "`n✨ Certificate creation complete!" -ForegroundColor Green
Write-Host "`n📋 Summary:"
Write-Host "   Windows Certificate: $WindowsCertPath"
Write-Host "   macOS Certificate:   $macOSCertPath"
Write-Host "   Password:            $CertPassword"
Write-Host "`n⚠️  IMPORTANT NOTES:"
Write-Host "   1. Store certificates and password in a SECURE location"
Write-Host "   2. For CI/CD, encode the PFX to Base64 and add to GitHub Secrets as:"
Write-Host "      - WINDOWS_CODE_SIGNING_CERTIFICATE (Base64 encoded PFX)"
Write-Host "      - MACOS_CODE_SIGNING_CERTIFICATE (Base64 encoded PFX)"
Write-Host "      - CODE_SIGNING_CERTIFICATE_PASSWORD"
Write-Host "   3. Never commit certificates to git"
Write-Host "   4. Users may see 'Unknown Publisher' until Windows learns to trust it"
Write-Host "`n🔗 Encode certificate to Base64:"
Write-Host "   [Convert]::ToBase64String([IO.File]::ReadAllBytes('$WindowsCertPath')) | Set-Clipboard"
Write-Host "`n"

