#!/bin/bash

# Multi-platform publish script for Dizimo GitHub Release
# Supports: Windows, Linux, and macOS (Intel and ARM)
# Usage: ./publish-release.sh [version] [release-notes]

set -e

VERSION_TAG="${1:-v1.0.0}"
RELEASE_NOTES="${2:-Release version ${VERSION_TAG}}"

echo "🚀 Starting Dizimo multi-platform release..."
echo "Version: $VERSION_TAG"
echo "Release Notes: $RELEASE_NOTES"
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Check Git repository
echo -e "${YELLOW}1. Checking for Git repository...${NC}"
if [ ! -d ".git" ]; then
    echo -e "${RED}❌ Not a Git repository!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Git repository found${NC}"
echo ""

# Check working tree is clean
echo -e "${YELLOW}2. Checking Git working tree...${NC}"
if [ -n "$(git status -s)" ]; then
    echo -e "${RED}❌ Working tree is not clean. Please commit all changes.${NC}"
    echo "Uncommitted changes:"
    git status -s
    exit 1
fi
echo -e "${GREEN}✅ Working tree is clean${NC}"
echo ""

# Build project
echo -e "${YELLOW}3. Building project (Release mode)...${NC}"
dotnet build -c Release
if [ $? -ne 0 ]; then
    echo -e "${RED}❌ Build failed!${NC}"
    exit 1
fi
echo -e "${GREEN}✅ Build completed successfully${NC}"
echo ""

# Define platforms
declare -a PLATFORMS=("win-x64" "linux-x64" "osx-x64" "osx-arm64")
declare -A PLATFORM_NAMES=([win-x64]="Windows" [linux-x64]="Linux" [osx-x64]="macOS Intel" [osx-arm64]="macOS ARM64")

echo -e "${YELLOW}4. Publishing for multiple platforms...${NC}"
echo ""

# Clean previous publish directory
if [ -d "publish" ]; then
    rm -rf publish
fi
mkdir -p publish

# Publish for each platform
for PLATFORM in "${PLATFORMS[@]}"; do
    PLATFORM_NAME="${PLATFORM_NAMES[$PLATFORM]}"
    echo -e "${CYAN}   📦 Publishing for $PLATFORM_NAME ($PLATFORM)...${NC}"
    
    PUBLISH_DIR="publish/Dizimo-$VERSION_TAG-$PLATFORM"
    
    dotnet publish Dizimo/Dizimo.csproj \
        -c Release \
        -o "$PUBLISH_DIR" \
        -r "$PLATFORM" \
        --self-contained true
    
    if [ $? -ne 0 ]; then
        echo -e "${RED}   ❌ Publish failed for $PLATFORM!${NC}"
        exit 1
    fi
    
    echo -e "${GREEN}   ✅ Published to: $PUBLISH_DIR${NC}"
    
    # Create ZIP archive
    ARCHIVE_NAME="Dizimo-$VERSION_TAG-$PLATFORM.zip"
    echo -e "${CYAN}   📦 Creating archive: $ARCHIVE_NAME${NC}"
    
    cd publish
    zip -r "../$ARCHIVE_NAME" "Dizimo-$VERSION_TAG-$PLATFORM" > /dev/null 2>&1
    cd ..
    
    if [ -f "$ARCHIVE_NAME" ]; then
        SIZE=$(ls -lh "$ARCHIVE_NAME" | awk '{print $5}')
        echo -e "${GREEN}   ✅ Archive created: $ARCHIVE_NAME ($SIZE)${NC}"
    else
        echo -e "${RED}   ❌ Failed to create archive!${NC}"
        exit 1
    fi
    
    echo ""
done

echo -e "${YELLOW}5. Creating Git tag...${NC}"

# Check if tag already exists
if git tag | grep -q "^$VERSION_TAG$"; then
    echo -e "${YELLOW}   ⚠️  Tag $VERSION_TAG already exists.${NC}"
    read -p "   Do you want to delete and recreate it? (y/n) " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        git tag -d "$VERSION_TAG"
        git push origin --delete "$VERSION_TAG" 2>/dev/null || true
        echo -e "${GREEN}   ✅ Old tag deleted${NC}"
    else
        echo -e "${YELLOW}   ⏭️  Operation cancelled${NC}"
        exit 0
    fi
fi

git tag -a "$VERSION_TAG" -m "Release: $VERSION_TAG

$RELEASE_NOTES"

git push origin "$VERSION_TAG"
echo -e "${GREEN}✅ Tag created and pushed to GitHub${NC}"
echo ""

# Summary
echo -e "${GREEN}═════════════════════════════════════════${NC}"
echo -e "${GREEN}✅ PREPARATION COMPLETE!${NC}"
echo -e "${GREEN}═════════════════════════════════════════${NC}"
echo ""

echo -e "${CYAN}📦 Archives created:${NC}"
for PLATFORM in "${PLATFORMS[@]}"; do
    ARCHIVE="Dizimo-$VERSION_TAG-$PLATFORM.zip"
    if [ -f "$ARCHIVE" ]; then
        SIZE=$(ls -lh "$ARCHIVE" | awk '{print $5}')
        echo -e "   ${GREEN}✅${NC} $ARCHIVE ($SIZE)"
    fi
done
echo ""

echo -e "${CYAN}📝 Git Tag:${NC} $VERSION_TAG"
echo ""

echo -e "${YELLOW}🚀 NEXT STEPS:${NC}"
echo ""
echo "1. Go to: https://github.com/henriquegfernandes/Dizimo/releases/new"
echo ""
echo "2. Fill in:"
echo -e "   Release title: ${CYAN}Dizimo $VERSION_TAG${NC}"
echo -e "   Tag: ${CYAN}Select '$VERSION_TAG'${NC}"
echo -e "   Description: ${CYAN}[Paste your release notes]${NC}"
echo ""
echo "3. Upload archives:"
for PLATFORM in "${PLATFORMS[@]}"; do
    ARCHIVE="Dizimo-$VERSION_TAG-$PLATFORM.zip"
    if [ -f "$ARCHIVE" ]; then
        echo -e "   ${CYAN}→ $ARCHIVE${NC}"
    fi
done
echo ""
echo "4. Mark 'This is a pre-release' if applicable"
echo "5. Click 'Publish release'"
echo ""
echo -e "${CYAN}Repository:${NC} https://github.com/henriquegfernandes/Dizimo"
echo ""

