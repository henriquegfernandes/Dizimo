<#
register-runner-task.ps1

Creates a Scheduled Task that runs the runner's run.cmd at system startup.
Usage:
  .\register-runner-task.ps1 -RunnerPath C:\actions-runner\run.cmd -TaskName GitHubActionsRunner

#>

param(
    [Parameter(Mandatory=$false)][string]$RunnerPath = "C:\actions-runner\run.cmd",
    [Parameter(Mandatory=$false)][string]$TaskName = "GitHubActionsRunner"
)

if (-not (Test-Path $RunnerPath)) {
    Write-Error "Runner executable not found at $RunnerPath"
    exit 1
}

$action = New-ScheduledTaskAction -Execute "C:\Windows\System32\cmd.exe" -Argument "/c \"$RunnerPath\""
$trigger = New-ScheduledTaskTrigger -AtStartup
Register-ScheduledTask -Action $action -Trigger $trigger -TaskName $TaskName -Description "Starts GitHub Actions Runner for Dizimo CI" -User "SYSTEM" -RunLevel Highest -Force

Write-Host "Scheduled Task '$TaskName' registered to run $RunnerPath at startup."
