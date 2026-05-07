#!/bin/bash

# Release Readiness Check Script for Dizimo
# This script verifies that the application is ready for release

BOLD='\033[1m'
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${CYAN}${BOLD}═════════════════════════════════════════${NC}"
echo -e "${CYAN}${BOLD}   Dizimo Release Readiness Check${NC}"
echo -e "${CYAN}${BOLD}═════════════════════════════════════════${NC}"
echo ""

CHECKS_PASSED=0
CHECKS_FAILED=0

# Function to run a check
run_check() {
    local check_name="$1"
    local check_command="$2"
    
    echo -n "Checking: $check_name... "
    
    if eval "$check_command" > /dev/null 2>&1; then
        echo -e "${GREEN}✅ PASS${NC}"
        ((CHECKS_PASSED++))
    else
        echo -e "${RED}❌ FAIL${NC}"
        ((CHECKS_FAILED++))
    fi
}

# Checks
echo -e "${YELLOW}1. Repository Checks${NC}"
run_check "Git repository initialized" "test -d .git"
run_check "Working tree is clean" "[ -z \"\$(git status -s)\" ]"
run_check "Remote 'origin' configured" "git remote -v | grep -q origin"
echo ""

echo -e "${YELLOW}2. Build Checks${NC}"
run_check "dotnet SDK installed" "dotnet --version > /dev/null"
run_check ".NET 10.0 framework available" "dotnet --list-sdks | grep -q 10.0"
run_check "Dizimo.csproj exists" "test -f Dizimo/Dizimo.csproj"
run_check "Solution builds successfully" "dotnet build -c Release > /dev/null 2>&1"
echo ""

echo -e "${YELLOW}3. Configuration Checks${NC}"
run_check "RuntimeIdentifiers configured" "grep -q 'RuntimeIdentifiers' Dizimo/Dizimo.csproj"
run_check "App icon configured" "grep -q 'ApplicationIcon' Dizimo/Dizimo.csproj"
run_check "App icon file exists" "test -f Dizimo/Resources/AppIcon/appicon.ico"
run_check "Version numbers set" "grep -q 'ApplicationDisplayVersion' Dizimo/Dizimo.csproj"
echo ""

echo -e "${YELLOW}4. Scripts Checks${NC}"
run_check "Bash publish script exists" "test -f publish-release.sh"
run_check "Bash script is executable" "test -x publish-release.sh"
run_check "PowerShell script exists" "test -f publish-release-multiplatform.ps1"
echo ""

echo -e "${YELLOW}5. Documentation Checks${NC}"
run_check "Release guide exists" "test -f RELEASE-GUIDE-PT-BR.md"
run_check "Readiness report exists" "test -f RELEASE-READINESS.md"
run_check "README exists" "test -f README.md"
echo ""

echo -e "${YELLOW}6. Dependencies Checks${NC}"
run_check "NuGet.config present" "test -f NuGet.config"
run_check "No unresolved packages" "dotnet restore --dry-run > /dev/null 2>&1"
echo ""

echo -e "${YELLOW}7. Multi-Platform Support${NC}"
run_check "Can publish for Windows" "dotnet publish Dizimo/Dizimo.csproj -c Release -r win-x64 --dry-run > /dev/null 2>&1"
run_check "Can publish for Linux" "dotnet publish Dizimo/Dizimo.csproj -c Release -r linux-x64 --dry-run > /dev/null 2>&1"
run_check "Can publish for macOS (Intel)" "dotnet publish Dizimo/Dizimo.csproj -c Release -r osx-x64 --dry-run > /dev/null 2>&1"
run_check "Can publish for macOS (ARM)" "dotnet publish Dizimo/Dizimo.csproj -c Release -r osx-arm64 --dry-run > /dev/null 2>&1"
echo ""

# Summary
echo -e "${CYAN}${BOLD}═════════════════════════════════════════${NC}"
echo -e "${BOLD}Summary:${NC}"
echo -e "  ${GREEN}✅ Passed: $CHECKS_PASSED${NC}"
echo -e "  ${RED}❌ Failed: $CHECKS_FAILED${NC}"
echo -e "${CYAN}${BOLD}═════════════════════════════════════════${NC}"
echo ""

if [ $CHECKS_FAILED -eq 0 ]; then
    echo -e "${GREEN}${BOLD}✅ All checks passed! Ready for release.${NC}"
    echo ""
    echo "Next steps:"
    echo "1. Update version in Dizimo/Dizimo.csproj (if needed)"
    echo "2. Commit and push changes"
    echo "3. Run: ./publish-release.sh v1.0.0 'Release notes'"
    echo "4. Upload archives to GitHub"
    exit 0
else
    echo -e "${RED}${BOLD}❌ Some checks failed. Please review the warnings above.${NC}"
    exit 1
fi


