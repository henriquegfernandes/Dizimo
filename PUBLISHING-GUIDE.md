# 📋 Manual Publishing Guide - Dizimo Beta Release

## ⚡ Quick Version (5 minutes)

### Step 1: Build and Package

```powershell
cd C:\Projects\Dizimo
.\publish-release.ps1
```

This will:
- ✅ Compile the project
- ✅ Create `publish/` folder with the app
- ✅ Generate ZIP file
- ✅ Create Git tag
- ✅ Push tag to GitHub

### Step 2: Create Release on GitHub (Manual)

1. **Access the link:**
   https://github.com/henriquegfernandes/Dizimo/releases/new

2. **Fill in the fields:**
   - **Release title:** `Dizimo v1.0.0-beta.1`
   - **Tag:** Select `v1.0.0-beta.1` (already created by the script)
   - **Description:** Paste your release notes
   - **Check:** Mark "This is a pre-release" 📌

3. **Upload the file:**
   - Click "Choose a file"
   - Select: `Dizimo-v1.0.0-beta.1-windows.zip`

4. **Publish:**
   - Click "Publish release"

✅ **Done! Release is live!**

---

## 📋 Complete Step-by-Step Process

### Initial Setup (first time)

Before you start, verify:

```powershell
# 1. Navigate to the project folder
cd C:\Projects\Dizimo

# 2. Check Git status
git status

# 3. Commit any pending changes
git add .
git commit -m "Preparation for beta 1 release"
git push origin main
```

### Step 1: Update Version (optional)

Edit `Dizimo\Dizimo.csproj`:

```xml
<PropertyGroup>
    <!-- Update to the new version -->
    <ApplicationDisplayVersion>1.0.0-beta.1</ApplicationDisplayVersion>
    <InformationalVersion>1.0.0-beta.1</InformationalVersion>
</PropertyGroup>
```

If you made changes:
```powershell
git add Dizimo/Dizimo.csproj
git commit -m "Bump version to 1.0.0-beta.1"
git push origin main
```

### Step 2: Build and Package

```powershell
cd C:\Projects\Dizimo
.\publish-release.ps1 -VersionTag "v1.0.0-beta.1" -ReleaseNotes "First beta version"
```

**Options:**

```powershell
# Version only (uses default notes)
.\publish-release.ps1 -VersionTag "v1.0.0-beta.2"

# With custom notes
.\publish-release.ps1 `
    -VersionTag "v1.0.0-beta.1" `
    -ReleaseNotes "Beta version 1 with backup support"

# Final version (not beta)
.\publish-release.ps1 -VersionTag "v1.0.0" -ReleaseNotes "Version 1.0.0 stable"
```

The script will:
- Compile in Release mode
- Create `publish/` folder
- Generate ZIP: `Dizimo-v1.0.0-beta.1-windows.zip`
- Create Git tag and push to GitHub

### Step 3: Create Release on GitHub (Manual)

**Option A: Via Browser (Recommended)**

1. Access: https://github.com/henriquegfernandes/Dizimo/releases/new

2. Fill in:
   ```
   Release title: Dizimo v1.0.0-beta.1
   
   Tag: v1.0.0-beta.1 (select from dropdown)
   
   Description:
   ## What's New 🎉
   
   - Automatic backup support
   - Improved interface
   - Bug fixes
   
   ## How to Test
   - Download the ZIP file
   - Extract to a folder
   - Run Dizimo.exe
   
   ## Feedback
   Please report bugs at: [Issues](https://github.com/henriquegfernandes/Dizimo/issues)
   ```

3. Mark: 📌 **This is a pre-release**

4. Click: "Attach binaries" or drag the ZIP file

5. Click: **"Publish release"**

**Option B: Via GitHub CLI (Advanced)**

If you have [GitHub CLI](https://cli.github.com/) installed:

```powershell
# Login (first time)
gh auth login

# Create release
gh release create v1.0.0-beta.1 \
    --title "Dizimo v1.0.0-beta.1" \
    --notes "First beta version" \
    --prerelease \
    Dizimo-v1.0.0-beta.1-windows.zip
```

---

## 🔄 Complete Example Flow

Real example of publication:

```powershell
# 1. Navigate to the folder
cd C:\Projects\Dizimo

# 2. Verify that everything is committed
git status
# (should show: working tree clean)

# 3. Run the publishing script
.\publish-release.ps1 -VersionTag "v1.0.0-beta.1" -ReleaseNotes "Beta 1 with UI improvements"

# Result:
# ✅ Compilation complete
# ✅ Application published
# ✅ ZIP file created: Dizimo-v1.0.0-beta.1-windows.zip
# ✅ Tag created: v1.0.0-beta.1
# ✅ Tag pushed to GitHub

# 4. Open GitHub in browser
Start-Process "https://github.com/henriquegfernandes/Dizimo/releases/new"

# 5. Fill in the form (as per instructions above)
# 6. Upload: Dizimo-v1.0.0-beta.1-windows.zip
# 7. Click "Publish release"

# 8. (Optional) View the releases page
Start-Process "https://github.com/henriquegfernandes/Dizimo/releases"
```

---

## 📝 Professional Release Notes

### Example Beta 1:

```markdown
## Dizimo v1.0.0-beta.1

### 🎯 Beta Goals
This is the first beta version of Dizimo. We are seeking feedback on:
- Overall stability
- Performance
- User interface

### 🎁 Main Features
- Tithe management
- Basic reports
- Local backup
- Multi-user support

### ⚠️ Known Issues
- [Issue #1] Charts may be slow with many records
- [Issue #2] Cloud backup not yet implemented

### 🙋 Testers Needed
We're looking for:
- Testers interested in providing feedback
- Bug reports
- Improvement suggestions

### 🐛 How to Report Bugs
1. Access: https://github.com/henriquegfernandes/Dizimo/issues
2. Click "New issue"
3. Describe the problem in detail

### 📥 Download
Available in: [Assets below](#assets)
```

### Example Beta 2:

```markdown
## Dizimo v1.0.0-beta.2

### 🎯 What Changed

#### ✅ Fixed
- Crash when opening charts with lots of data
- Synchronization issue when adding user
- Slowness when filtering tithes

#### ✨ New
- CSV export
- Dark theme
- Keyboard shortcuts

#### 📈 Improved
- 40% improvement in loading performance
- Reports interface
- Field validation

### 📊 Statistics
- 15 issues resolved
- 3 new features
- 200+ lines of code refactored

### 👏 Thanks
Thanks to beta testers who reported bugs!
```

---

## 🔧 Troubleshooting

### ❌ Error: "Tag already exists"

```powershell
# Delete local tag
git tag -d v1.0.0-beta.1

# Delete on GitHub
git push origin --delete v1.0.0-beta.1

# Recreate
.\publish-release.ps1 -VersionTag "v1.0.0-beta.1"
```

### ❌ Error: "ZIP file not found"

```powershell
# Check if it was created
ls Dizimo-v*.zip

# If it doesn't exist, the script had a compilation error
# Check the output above
```

### ❌ Cannot upload to GitHub interface

**Solution A: Drag and drop**
- Drag the ZIP directly into the description field

**Solution B: Upload button**
- Look for "Attach binaries" or "Add files"

**Solution C: GitHub CLI**
```powershell
gh release upload v1.0.0-beta.1 Dizimo-v1.0.0-beta.1-windows.zip
```

### ❌ Release doesn't appear

- Check that you clicked "Publish release" (not "Save as draft")
- Wait a few seconds and refresh the page
- Confirm it's a "pre-release" (not draft)

---

## ✅ Publication Checklist

Before each release:

- [ ] Tests executed locally
- [ ] Code committed and pushed (git push)
- [ ] Version updated in `.csproj`
- [ ] Release notes prepared
- [ ] Script executed successfully
- [ ] ZIP file verified
- [ ] Release created on GitHub
- [ ] ZIP file uploaded
- [ ] Release marked as "pre-release"
- [ ] Link shared with testers

---

## 🚀 Next Versions

For beta 2, 3, etc:

```powershell
.\publish-release.ps1 -VersionTag "v1.0.0-beta.2" -ReleaseNotes "Beta 2 with fixes"
.\publish-release.ps1 -VersionTag "v1.0.0-beta.3" -ReleaseNotes "Beta 3 final"
```

For final version:

```powershell
.\publish-release.ps1 -VersionTag "v1.0.0" -ReleaseNotes "Version 1.0 stable"
```

---

## 💬 Support

Have questions?
- GitHub Issues: https://github.com/henriquegfernandes/Dizimo/issues
- GitHub Discussions: https://github.com/henriquegfernandes/Dizimo/discussions
