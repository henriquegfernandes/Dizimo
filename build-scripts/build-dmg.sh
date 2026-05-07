#!/bin/bash
# Build DMG (Disk Image) Installer for Dizimo on macOS
# Usage: ./build-dmg.sh <publish_dir> <app_version>

set -e

PUBLISH_DIR="${1:-.}"
APP_VERSION="${2:-2.0.0}"
APP_NAME="Dizimo"
OUTPUT_DIR="."
DMG_NAME="${APP_NAME}-${APP_VERSION}.dmg"
TEMP_DMG="temp-${DMG_NAME}"

echo "Building macOS DMG for $APP_NAME v$APP_VERSION..."

# Create temporary directory structure
TEMP_DIR="/tmp/Dizimo-DMG-$$"
mkdir -p "$TEMP_DIR/$APP_NAME"

# Create the app bundle structure
APP_BUNDLE="$TEMP_DIR/$APP_NAME/$APP_NAME.app"
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copy application files
cp -r "$PUBLISH_DIR"/* "$APP_BUNDLE/Contents/MacOS/"
chmod +x "$APP_BUNDLE/Contents/MacOS/Dizimo"

# Create Info.plist
cat > "$APP_BUNDLE/Contents/Info.plist" << EOF
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleExecutable</key>
    <string>Dizimo</string>
    <key>CFBundleIdentifier</key>
    <string>com.henriquefernandestech.dizimo</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>Dizimo</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>$APP_VERSION</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
EOF

# Create symlink to Applications folder
ln -s /Applications "$TEMP_DIR/$APP_NAME/Applications"

# Create DMG
echo "Creating DMG image..."
hdiutil create -volname "$APP_NAME" -srcfolder "$TEMP_DIR/$APP_NAME" -ov -format UDZO "$TEMP_DMG"

# Move to output directory
mv "$TEMP_DMG" "$OUTPUT_DIR/$DMG_NAME"

# Clean up
rm -rf "$TEMP_DIR"

echo "DMG built successfully!"
ls -lh "$OUTPUT_DIR/$DMG_NAME"

