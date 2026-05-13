#!/bin/bash
# Centralized build configuration for Dizimo
# This file contains common paths and settings used across all build scripts

# Project root directory (script directory parent)
export PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

# Icon path (relative to PROJECT_ROOT)
export ICON_PATH="${PROJECT_ROOT}/Dizimo/Resources/AppIcon/appicon.ico"

# Application metadata
export APP_NAME="Dizimo"
export APP_IDENTIFIER="com.henriquefernandestech.dizimo"
export APP_PUBLISHER="Henrique Fernandes Tech"

# Validate that icon exists
if [ ! -f "$ICON_PATH" ]; then
    echo "❌ Error: Icon file not found at: $ICON_PATH" >&2
    exit 1
fi

echo "✅ Build configuration loaded"
echo "   Project Root: $PROJECT_ROOT"
echo "   Icon Path: $ICON_PATH"

