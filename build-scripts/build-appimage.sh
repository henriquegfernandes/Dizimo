#!/bin/bash
# Build AppImage for Dizimo on Linux
# Usage: ./build-appimage.sh <publish_dir> <app_version>

set -e

PUBLISH_DIR="${1:-.}"
APP_VERSION="${2:-2.0.0}"
APP_NAME="Dizimo"
OUTPUT_DIR="."

echo "Building AppImage for $APP_NAME v$APP_VERSION..."

# Create AppDir structure
mkdir -p AppDir/usr/bin
mkdir -p AppDir/usr/lib
mkdir -p AppDir/usr/share/applications
mkdir -p AppDir/usr/share/icons/hicolor/256x256/apps

# Copy application files
cp -r "$PUBLISH_DIR"/* AppDir/usr/bin/ || true
chmod +x AppDir/usr/bin/Dizimo

# Create desktop entry
cat > AppDir/usr/share/applications/Dizimo.desktop << EOF
[Desktop Entry]
Type=Application
Name=Dizimo
Exec=Dizimo
Icon=Dizimo
Categories=Utility;
EOF

# Create AppRun script
cat > AppDir/AppRun << 'APPRUN_EOF'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
export LD_LIBRARY_PATH="$HERE/usr/lib:$LD_LIBRARY_PATH"
exec "$HERE/usr/bin/Dizimo" "$@"
APPRUN_EOF
chmod +x AppDir/AppRun

# Create AppImage
echo "Creating AppImage..."
if command -v appimagetool &> /dev/null; then
    appimagetool AppDir "${OUTPUT_DIR}/${APP_NAME}-${APP_VERSION}-x86_64.AppImage"
else
    echo "WARNING: appimagetool not found. Installing runtime..."
    # Fallback: Create a simple wrapper script
    cat > "${OUTPUT_DIR}/${APP_NAME}.AppImage" << 'WRAPPER_EOF'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
HERE="${HERE}/AppDir"
exec "$HERE/AppRun" "$@"
WRAPPER_EOF
    chmod +x "${OUTPUT_DIR}/${APP_NAME}.AppImage"
fi

echo "AppImage built successfully!"
ls -lh "${OUTPUT_DIR}"/${APP_NAME}*.AppImage

