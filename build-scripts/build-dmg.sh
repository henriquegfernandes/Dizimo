#!/bin/bash
# Build DMG (Disk Image) Installer for Dizimo on macOS
# Usage: ./build-dmg.sh <publish_dir> <app_version>

set -e

PUBLISH_DIR="${1:-.}"
APP_VERSION="${2:-1.1.2}"
APP_NAME="Dizimo"
OUTPUT_DIR="."
DMG_NAME="${APP_NAME}-${APP_VERSION}.dmg"
TEMP_DMG="temp-${DMG_NAME}"
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

echo "Building macOS DMG for $APP_NAME v$APP_VERSION..."

# Create temporary directory structure
TEMP_DIR="/tmp/Dizimo-DMG-$$"
mkdir -p "$TEMP_DIR/$APP_NAME"

# Create the app bundle structure
APP_BUNDLE="$TEMP_DIR/$APP_NAME/$APP_NAME.app"
mkdir -p "$APP_BUNDLE/Contents/MacOS"
mkdir -p "$APP_BUNDLE/Contents/Resources"

# Copy application files
echo "Copying application files..."
cp -r "$PUBLISH_DIR"/* "$APP_BUNDLE/Contents/MacOS/"
chmod +x "$APP_BUNDLE/Contents/MacOS/Dizimo"

# Copy and convert icon
echo "Setting up application icon..."
if [ -f "$PROJECT_ROOT/Dizimo/Resources/AppIcon/appicon.ico" ]; then
    # Convert ICO to ICNS if iconutil is available
    if command -v iconutil &> /dev/null; then
        # Create .iconset directory
        ICONSET="$TEMP_DIR/dizimo.iconset"
        mkdir -p "$ICONSET"
        
        # Try to create ICNS from SVG or PNG
        if [ -f "$PROJECT_ROOT/Dizimo/Resources/AppIcon/appicon.svg" ]; then
            echo "Converting SVG icon to ICNS..."
            # This requires additional tools, so we'll use a simpler approach
            cp "$PROJECT_ROOT/Dizimo/Resources/AppIcon/appicon.svg" "$APP_BUNDLE/Contents/Resources/AppIcon.svg" 2>/dev/null || true
        fi
        
        if [ -f "$PROJECT_ROOT/Dizimo/Resources/AppIcon/dizimoicon.png" ]; then
            cp "$PROJECT_ROOT/Dizimo/Resources/AppIcon/dizimoicon.png" "$APP_BUNDLE/Contents/Resources/AppIcon.png"
        fi
    fi
    
    # Also copy ICO as fallback
    cp "$PROJECT_ROOT/Dizimo/Resources/AppIcon/appicon.ico" "$APP_BUNDLE/Contents/Resources/AppIcon.ico" 2>/dev/null || true
fi

# Create Info.plist with CFBundleIconFile and proper metadata
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
    <key>NSRequiresIPhoneOS</key>
    <false/>
    <key>NSHumanReadableCopyright</key>
    <string>Copyright © 2024 Henrique Fernandes Tech. All rights reserved.</string>
</dict>
</plist>
EOF

# Replace version placeholder
sed -i '' "s/VERSION_PLACEHOLDER/$APP_VERSION/g" "$APP_BUNDLE/Contents/Info.plist"

# Create PkgInfo file
echo "APPL????" > "$APP_BUNDLE/Contents/PkgInfo"

# Create a symbolic link to Applications folder for drag-and-drop install
echo "Creating Applications symlink..."
ln -s /Applications "$TEMP_DIR/$APP_NAME/Applications"

# Create a .DS_Store with custom layout (optional, requires tools)
# For now, we'll just create the structure

# Create DMG
echo "Creating DMG image..."
hdiutil create \
    -volname "$APP_NAME" \
    -srcfolder "$TEMP_DIR/$APP_NAME" \
    -ov \
    -format UDZO \
    "$TEMP_DMG"

# Move to output directory
mv "$TEMP_DMG" "$OUTPUT_DIR/$DMG_NAME"

# Make the DMG world-readable
chmod 644 "$OUTPUT_DIR/$DMG_NAME"

# Clean up
rm -rf "$TEMP_DIR"

echo ""
echo "✅ DMG built successfully!"
echo "   File: $OUTPUT_DIR/$DMG_NAME"
echo "   Size: $(du -h "$OUTPUT_DIR/$DMG_NAME" | cut -f1)"
echo ""
echo "To install:"
echo "  1. Open $DMG_NAME"
echo "  2. Drag 'Dizimo' to the Applications folder"
echo "  3. Eject the DMG"
echo "  4. Open Applications and launch Dizimo"
