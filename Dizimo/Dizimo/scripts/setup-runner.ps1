<#
setup-runner.ps1

Simple helper to download, extract and configure a GitHub Actions self-hosted runner for Windows.
Usage:
  .\setup-runner.ps1 -Owner your-org -Repo Dizimo -Token YOUR_TOKEN -RunnerName my-runner

#>

param(
    [Parameter(Mandatory=$true)][string]$Owner,
    [Parameter(Mandatory=$true)][string]$Repo,
    [Parameter(Mandatory=$true)][string]$Token,
    [Parameter(Mandatory=$false)][string]$RunnerName = "$env:COMPUTERNAME-runner",
    [Parameter(Mandatory=$false)][string]$WorkDir = "C:\actions-runner",
    [Parameter(Mandatory=$false)][string]$Version = "latest"
)

Set-Location -Path $env:TEMP
if (-not (Test-Path $WorkDir)) { New-Item -ItemType Directory -Path $WorkDir -Force | Out-Null }
Set-Location -Path $WorkDir

Write-Host "Preparing GitHub Actions Runner in: $WorkDir"

if ($Version -eq 'latest') {
    # determine latest release download URL
    $api = "https://api.github.com/repos/actions/runner/releases/latest"
    $json = Invoke-RestMethod -Uri $api -UseBasicParsing
    $tag = $json.tag_name
    $asset = $json.assets | Where-Object { $_.name -like '*win-x64*.zip' } | Select-Object -First 1
    $url = $asset.browser_download_url
} else {
    # User-provided version stub
    $url = "https://github.com/actions/runner/releases/download/v$Version/actions-runner-win-x64-$Version.zip"
}

Write-Host "Downloading runner from: $url"
$zip = Join-Path $WorkDir "actions-runner.zip"
Invoke-WebRequest -Uri $url -OutFile $zip -UseBasicParsing
Expand-Archive -Path $zip -DestinationPath $WorkDir -Force
Remove-Item $zip -Force

Write-Host "Configuring runner for $Owner/$Repo as $RunnerName"
& .\config.cmd --url "https://github.com/$Owner/$Repo" --token $Token --name $RunnerName --unattended --replace

Write-Host "Runner configured. Test by running: .\run.cmd"
