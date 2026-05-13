#!/bin/bash
# Build DMG (Disk Image) Installer for Dizimo on macOS
# Usage: ./build-dmg.sh <publish_dir> <app_version>

set -e

PUBLISH_DIR="${1:-.}"
APP_VERSION="${2:-1.1.2}"
OUTPUT_DIR="."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

# Source centralized build configuration
source "$SCRIPT_DIR/build-config.sh"

# Define DMG variables
DMG_NAME="${APP_NAME}-${APP_VERSION}.dmg"
TEMP_DMG="temp-${DMG_NAME}"
TEMP_DIR="/tmp/Dizimo-DMG-$$"

echo "Building macOS DMG for $APP_NAME v$APP_VERSION..."

# Create temporary directory structure
mkdir -p "$TEMP_DIR/$APP_NAME"

# Create the app bundle structure
APP_BUNDLE="$TEMP_DIR/$APP_NAME/$APP_NAME.app"
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copy application files
echo "Copying application files..."
cp -r "$PUBLISH_DIR"/* "$APP_BUNDLE/Contents/MacOS/"
chmod +x "$APP_BUNDLE/Contents/MacOS/Dizimo"

# Copy icon from centralized location
echo "Setting up application icon..."
if [ -f "$ICON_PATH" ]; then
    cp "$ICON_PATH" "$APP_BUNDLE/Contents/Resources/AppIcon.ico"
    echo "✅ Icon copied to app bundle"
else
    echo "⚠️  Warning: Icon not found at $ICON_PATH"
fi

# Create Info.plist with icon reference and proper metadata
echo "Creating Info.plist..."
cat > "$APP_BUNDLE/Contents/Info.plist" << 'EOF'
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
    <key>CFBundleDisplayName</key>
    <string>Dizimo</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>VERSION_PLACEHOLDER</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.15</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSSupportsAutomaticGraphicsSwitching</key>
    <true/>
    <key>NSHumanReadableCopyright</key>
    <string>Copyright © 2024 Henrique Fernandes Tech. All rights reserved.</string>
</dict>
</plist>
EOF

# Replace version placeholder
sed -i '' "s/VERSION_PLACEHOLDER/$APP_VERSION/g" "$APP_BUNDLE/Contents/Info.plist"

# Create PkgInfo file
echo "APPL????" > "$APP_BUNDLE/Contents/PkgInfo"

# Create Applications symlink for drag-and-drop install
ln -s /Applications "$TEMP_DIR/$APP_NAME/Applications"

# Create DMG image
echo "Creating DMG image..."
hdiutil create \
    -volname "$APP_NAME" \
    -srcfolder "$TEMP_DIR/$APP_NAME" \
    -ov \
    -format UDZO \
    "$TEMP_DMG"

# Move to output directory
mv "$TEMP_DMG" "$OUTPUT_DIR/$DMG_NAME"
chmod 644 "$OUTPUT_DIR/$DMG_NAME"

# Clean up
rm -rf "$TEMP_DIR"

echo ""
echo "✅ DMG built successfully!"
echo "   File: $OUTPUT_DIR/$DMG_NAME"
echo "   Size: $(du -h "$OUTPUT_DIR/$DMG_NAME" | cut -f1)"
