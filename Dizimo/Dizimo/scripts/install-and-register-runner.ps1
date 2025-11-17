<#
install-and-register-runner.ps1

Wrapper that downloads/configures the GitHub Actions runner and registers a Scheduled Task to run it at startup.
Usage:
  .\install-and-register-runner.ps1 -Owner your-org -Repo Dizimo -Token YOUR_TOKEN

# This script assumes setup-runner.ps1 and register-runner-task.ps1 exist in the same folder.
# It must be run as Administrator to register the Scheduled Task.

param(
    [Parameter(Mandatory=$true)][string]$Owner,
    [Parameter(Mandatory=$true)][string]$Repo,
    [Parameter(Mandatory=$true)][string]$Token,
    [Parameter(Mandatory=$false)][string]$RunnerName = "$env:COMPUTERNAME-runner",
    [Parameter(Mandatory=$false)][string]$WorkDir = "C:\actions-runner"
)

Push-Location -Path (Split-Path -Parent $MyInvocation.MyCommand.Definition)

Write-Host "Running setup-runner.ps1..."
& .\setup-runner.ps1 -Owner $Owner -Repo $Repo -Token $Token -RunnerName $RunnerName -WorkDir $WorkDir

Write-Host "Registering Scheduled Task..."
& .\register-runner-task.ps1 -RunnerPath (Join-Path $WorkDir 'run.cmd') -TaskName 'GitHubActionsRunner'

Write-Host "Runner setup complete. Verify with: Get-ScheduledTask -TaskName 'GitHubActionsRunner' and check runner logs in $WorkDir"

Pop-Location
