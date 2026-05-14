#!/bin/bash
# Sign macOS app bundle and DMG
# Usage: ./sign-macos-binary.sh -p /path/to/app.bundle -i com.example.app -c "Developer ID Application"

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

# Default values
CERT_IDENTITY=""
APP_BUNDLE=""
BUNDLE_ID=""
ENTITLEMENTS=""

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        -p|--path)
            APP_BUNDLE="$2"
            shift 2
            ;;
        -i|--bundle-id)
            BUNDLE_ID="$2"
            shift 2
            ;;
        -c|--cert-identity)
            CERT_IDENTITY="$2"
            shift 2
            ;;
        -e|--entitlements)
            ENTITLEMENTS="$2"
            shift 2
            ;;
        *)
            echo "Unknown option: $1"
            exit 1
            ;;
    esac
done

if [ -z "$APP_BUNDLE" ] || [ -z "$CERT_IDENTITY" ]; then
    echo "Usage: $0 -p /path/to/app.bundle -c 'Developer ID Application' [-i bundle.id] [-e entitlements.plist]"
    echo ""
    echo "Options:"
    echo "  -p, --path              Path to app bundle (required)"
    echo "  -c, --cert-identity     Certificate identity (required)"
    echo "  -i, --bundle-id         Bundle ID (optional)"
    echo "  -e, --entitlements      Entitlements plist file (optional)"
    echo ""
    echo "Example:"
    echo "  $0 -p \"Dizimo.app\" -c \"Developer ID Application: Your Name\" -i com.example.dizimo"
    exit 1
fi

echo "🔐 Signing macOS app bundle..." 
echo "   Path: $APP_BUNDLE"
echo "   Identity: $CERT_IDENTITY"
[ -n "$BUNDLE_ID" ] && echo "   Bundle ID: $BUNDLE_ID"

if [ ! -d "$APP_BUNDLE" ]; then
    echo "❌ Error: App bundle not found: $APP_BUNDLE"
    exit 1
fi

# Check if codesign is available
if ! command -v codesign &> /dev/null; then
    echo "❌ Error: codesign not found. This is required on macOS."
    exit 1
fi

# Create entitlements file if not provided
if [ -z "$ENTITLEMENTS" ]; then
    ENTITLEMENTS=$(mktemp)
    cat > "$ENTITLEMENTS" << 'EOF'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.cs.allow-unsigned-executable-memory</key>
    <true/>
    <key>com.apple.security.cs.allow-dyld-environment-variables</key>
    <true/>
</dict>
</plist>
EOF
    echo "✅ Generated entitlements file: $ENTITLEMENTS"
fi

# Sign app bundle
echo ""
echo "⏳ Signing app bundle..."

# First, sign frameworks and dependencies
if [ -d "$APP_BUNDLE/Contents/Frameworks" ]; then
    echo "   Signing frameworks..."
    find "$APP_BUNDLE/Contents/Frameworks" -type f -name "*.dylib" -o -name "*.framework" | while read -r framework; do
        codesign --force --verify --verbose \
            --sign "$CERT_IDENTITY" \
            --timestamp \
            --entitlements "$ENTITLEMENTS" \
            "$framework" 2>/dev/null || true
    done
fi

# Sign the main bundle
codesign --force --verify --verbose \
    --sign "$CERT_IDENTITY" \
    --timestamp \
    --entitlements "$ENTITLEMENTS" \
    "$APP_BUNDLE"

if [ $? -eq 0 ]; then
    echo "✅ App bundle signed successfully!"
else
    echo "❌ Error signing app bundle"
    exit 1
fi

# Verify signature
echo ""
echo "✅ Verifying signature..."
codesign --verify --verbose "$APP_BUNDLE"

if [ $? -eq 0 ]; then
    echo "✨ Signature verified successfully!"
    echo ""
    echo "📋 Signature Details:"
    codesign -dv "$APP_BUNDLE"
else
    echo "❌ Signature verification failed!"
    exit 1
fi

# Clean up temporary entitlements if we created one
if [ -z "$ENTITLEMENTS" ] || [ "$ENTITLEMENTS" = "$(mktemp)" ]; then
    rm -f "$ENTITLEMENTS"
fi

echo ""
echo "✨ macOS app bundle signed successfully!"

