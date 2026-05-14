# Code Signing Setup Guide

This guide walks you through setting up code signing for Dizimo releases.

## Quick Start

### 1. Generate Self-Signed Certificates (5 minutes)

```powershell
cd C:\Projects\Dizimo
./build-scripts/create-certificates.ps1
```

Output:
- ✅ `certs/dizimo-codesign.pfx` - Windows installer certificate
- ✅ `certs/dizimo-macos-codesign.pfx` - macOS app certificate

### 2. Configure GitHub Secrets (3 minutes)

**Step 1**: Encode Windows certificate to Base64

```powershell
# Copy Base64 to clipboard
[Convert]::ToBase64String([IO.File]::ReadAllBytes('C:\Projects\Dizimo\certs\dizimo-codesign.pfx')) | Set-Clipboard

# Or save to file
[Convert]::ToBase64String([IO.File]::ReadAllBytes('C:\Projects\Dizimo\certs\dizimo-codesign.pfx')) | Out-File "windows-cert.txt"
```

**Step 2**: Encode macOS certificate to Base64

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes('C:\Projects\Dizimo\certs\dizimo-macos-codesign.pfx')) | Set-Clipboard
```

**Step 3**: Add to GitHub Secrets

Go to: **Repository → Settings → Secrets and variables → Actions**

Create 3 new secrets:

| Secret Name | Value |
|-------------|-------|
| `WINDOWS_CODE_SIGNING_CERTIFICATE` | Base64 encoded Windows PFX |
| `MACOS_CODE_SIGNING_CERTIFICATE` | Base64 encoded macOS PFX |
| `CODE_SIGNING_CERTIFICATE_PASSWORD` | Certificate password (from create-certificates.ps1) |

⚠️ **IMPORTANT**: Never share these secrets or commit them to git!

### 3. Done! 🎉

Next release will automatically:
- ✅ Sign Windows installer
- ✅ Sign macOS app bundle
- ✅ Create Linux AppImage checksums
- ✅ Reduce SmartScreen warnings on Windows

---

## What Gets Signed

### Windows
- **File**: `Dizimo-v*.exe` (installer)
- **Tool**: SignTool (Windows SDK)
- **Effect**: Reduces SmartScreen warnings
- **Trust Level**: Builds gradually with downloads

### macOS
- **File**: `Dizimo.app`, `Dizimo-v*.dmg`
- **Tool**: codesign (macOS built-in)
- **Effect**: Enables app notarization
- **Trust Level**: Recognized as legitimate

### Linux
- **File**: `Dizimo-v*.AppImage`
- **Security**: SHA256 checksums provided
- **Effect**: Users can verify integrity

---

## Testing Locally

### Test Windows Signing

```powershell
# Sign an EXE locally
./build-scripts/sign-windows-binary.ps1 `
    -FilePath "Dizimo.exe" `
    -CertPath "certs/dizimo-codesign.pfx" `
    -CertPassword "YourPassword"

# Verify signature
signtool verify /pa Dizimo.exe
```

### Test macOS Signing

```bash
# Sign app bundle locally (requires macOS)
./build-scripts/sign-macos-binary.sh \
    -p "Dizimo.app" \
    -c "Your Certificate Identity"

# Verify signature
codesign -dv Dizimo.app
```

---

## Certificate Details

### Current: Self-Signed Certificate (Development)

✅ **Pros:**
- Free
- Immediate
- Works for testing

❌ **Cons:**
- Windows shows "Unknown Publisher"
- Trust builds slowly
- Users may distrust

### Recommended: Commercial Certificate (Production)

When ready for production release:

**Popular providers:**
- **DigiCert** - $99/year (recommended)
- **Sectigo** - $79/year
- **Comodo** - $79/year

**Benefits:**
- ✅ Immediate Windows SmartScreen trust
- ✅ Professional appearance
- ✅ Higher user confidence

**Steps:**
1. Purchase certificate
2. Install locally and export to PFX
3. Update GitHub secrets
4. Done - all releases will use it

---

## Security & Best Practices

### ✅ DO

- ✅ Store certificate password securely
- ✅ Use GitHub Secrets for sensitive data
- ✅ Backup certificates in secure location
- ✅ Keep certificate password updated  in password manager
- ✅ Rotate certificates annually

### ❌ DON'T

- ❌ Commit certificates to git (`.gitignore` prevents this)
- ❌ Store passwords in code
- ❌ Share secrets publicly
- ❌ Use same certificate for multiple apps
- ❌ Leave certificates on public machines

---

## Troubleshooting

### "Unknown Publisher" Warning

**Cause**: Self-signed certificate or new commercial certificate

**Solution**:
1. User clicks "Run anyway" → builds trust
2. After ~50 downloads: Warning reduces
3. After ~500 downloads: Warning disappears
4. Consider: Commercial certificate ($99/year)

### Signing Fails in CI/CD

**Check**:
1. GitHub Secrets are configured
2. Certificate password is correct
3. Certificate file is valid

**Debug**:
```powershell
# Verify certificate locally
$cert = Get-PfxCertificate -FilePath "certs/dizimo-codesign.pfx" -Password (ConvertTo-SecureString -String "password" -AsPlainText -Force)
Write-Host $cert.Subject
Write-Host $cert.Thumbprint
```

### "SignTool not found"

**Solution** (Windows 10/11):
```powershell
# Install Windows SDK
# https://developer.microsoft.com/en-us/windows/downloads/windows-sdk/

# Or find SignTool
Get-Command signtool.exe
```

---

## References

- **Windows Code Signing**: https://docs.microsoft.com/en-us/windows/win32/seccodeauth/using-signtool-to-sign-a-file
- **macOS Code Signing**: https://developer.apple.com/support/code-signing/
- **SmartScreen**: https://docs.microsoft.com/en-us/windows/security/threat-protection/
- **DigiCert Certificates**: https://www.digicert.com/
- **Certificate Management**: https://learn.microsoft.com/en-us/dotnet/framework/wcf/feature-details/working-with-certificates

---

## Next Steps

1. ✅ Run `create-certificates.ps1`
2. ✅ Add secrets to GitHub
3. ✅ Create a test release (push to main) 
4. ✅ Download and verify signing works
5. ✅ Share release with users
6. ✅ Monitor SmartScreen reputation

---

**Questions?** Check `CODE-SIGNING.md` for detailed information.

