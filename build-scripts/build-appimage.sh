#!/bin/bash
# Build AppImage for Dizimo on Linux
# Usage: ./build-appimage.sh <publish_dir> <app_version>

set -e

PUBLISH_DIR="${1:-.}"
APP_VERSION="${2:-1.1.2}"
APP_NAME="Dizimo"
OUTPUT_DIR="."
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(dirname "$SCRIPT_DIR")"

echo "Building AppImage for $APP_NAME v$APP_VERSION..."

# Create AppDir structure
mkdir -p AppDir/usr/bin
mkdir -p AppDir/usr/lib
mkdir -p AppDir/usr/share/applications
mkdir -p AppDir/usr/share/icons/hicolor/256x256/apps
mkdir -p AppDir/usr/share/icons/hicolor/512x512/apps

# Copy application files
cp -r "$PUBLISH_DIR"/* AppDir/usr/bin/ || true
chmod +x AppDir/usr/bin/Dizimo

# Copy icon files (ICO como fallback, PNG como principal)
if [ -f "$PROJECT_ROOT/Dizimo/Resources/AppIcon/appicon.ico" ]; then
    echo "Copiando ícone ICO..."
    cp "$PROJECT_ROOT/Dizimo/Resources/AppIcon/appicon.ico" AppDir/usr/share/icons/hicolor/256x256/apps/dizimo.ico
    cp "$PROJECT_ROOT/Dizimo/Resources/AppIcon/appicon.ico" AppDir/usr/share/icons/hicolor/512x512/apps/dizimo.ico
fi

if [ -f "$PROJECT_ROOT/Dizimo/Resources/AppIcon/dizimoicon.png" ]; then
    echo "Copiando ícone PNG..."
    cp "$PROJECT_ROOT/Dizimo/Resources/AppIcon/dizimoicon.png" AppDir/usr/share/icons/hicolor/256x256/apps/dizimo.png
    cp "$PROJECT_ROOT/Dizimo/Resources/AppIcon/dizimoicon.png" AppDir/usr/share/icons/hicolor/512x512/apps/dizimo.png
fi

# Create desktop entry com referência a ICO
cat > AppDir/usr/share/applications/dizimo.desktop << 'EOF'
[Desktop Entry]
Version=1.0
Type=Application
Name=Dizimo
Comment=Sistema de Controle Financeiro para Igrejas
Comment[pt_BR]=Sistema de Controle Financeiro para Igrejas
Exec=Dizimo %F
Icon=dizimo
Categories=Office;Finance;Qt;
Keywords=church;tithe;finance;offering;
Terminal=false
StartupNotify=true
StartupWMClass=Dizimo
EOF
... existing code...

# Create AppRun script
cat > AppDir/AppRun << 'APPRUN_EOF'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
export LD_LIBRARY_PATH="$HERE/usr/lib:$LD_LIBRARY_PATH"
export XDG_DATA_DIRS="$HERE/usr/share:${XDG_DATA_DIRS}"
exec "$HERE/usr/bin/Dizimo" "$@"
APPRUN_EOF
chmod +x AppDir/AppRun

# Create AppImage
echo "Creating AppImage..."
if command -v appimagetool &> /dev/null; then
    appimagetool AppDir "${OUTPUT_DIR}/${APP_NAME}-${APP_VERSION}-x86_64.AppImage"
else
    echo "⚠️  appimagetool not found. Criando AppImage com fallback..."
    cat > "${OUTPUT_DIR}/${APP_NAME}.AppImage" << 'WRAPPER_EOF'
#!/bin/bash
SELF=$(readlink -f "$0")
HERE=${SELF%/*}
HERE="${HERE}/AppDir"
exec "$HERE/AppRun" "$@"
WRAPPER_EOF
    chmod +x "${OUTPUT_DIR}/${APP_NAME}.AppImage"
fi

echo "✅ AppImage created successfully!"
ls -lh "${OUTPUT_DIR}"/${APP_NAME}*.AppImage

