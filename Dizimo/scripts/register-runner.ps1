<#
PowerShell script to download and configure a GitHub Actions self-hosted runner.
You must provide the repository URL and a registration token (see README instructions to obtain the token).

Usage (run as Administrator in C:\actions-runner):
 .\register-runner.ps1 -RepoUrl 'https://github.com/owner/repo' -Token 'YOUR_TOKEN'
#>

param(
    [Parameter(Mandatory=$true)] [string] $RepoUrl,
    [Parameter(Mandatory=$true)] [string] $Token
)

Set-StrictMode -Version Latest

$runnerDir = 'C:\actions-runner'
if (-not (Test-Path $runnerDir)) { New-Item -ItemType Directory -Path $runnerDir -Force }
Set-Location $runnerDir

Write-Host "Downloading latest runner..."
$arch = 'x64'
$ver = (Invoke-RestMethod -Uri 'https://api.github.com/repos/actions/runner/releases/latest').tag_name
$zipUrl = "https://github.com/actions/runner/releases/download/$ver/actions-runner-win-$arch-$ver.zip"

Write-Host "Fetching $zipUrl"
Invoke-WebRequest -Uri $zipUrl -OutFile 'actions-runner.zip'
Expand-Archive -Path 'actions-runner.zip' -DestinationPath $runnerDir -Force

Write-Host "Configuring runner (labels: self-hosted, windows, windows-maui)..."
.
# Configure using the provided repo URL and token
.
Write-Host "Please run the following command manually (paste into PowerShell) to register the runner:" 
Write-Host "`.\config.cmd --url $RepoUrl --token $Token --labels self-hosted,windows,windows-maui --name $(hostname)`"

Write-Host "After registration, run `.\run.cmd` to start the runner interactively, or install as a service per runner docs." 
