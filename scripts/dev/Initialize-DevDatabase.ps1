<#
.SYNOPSIS
    Creates the LocalDB instance and the HRMS databases on a developer PC, then applies all scripts.
.DESCRIPTION
    Needs SQL Server Express LocalDB (included with the Visual Studio data workload) and the .NET 10 SDK.
    Safe to run again: only pending scripts are applied.
.EXAMPLE
    .\Initialize-DevDatabase.ps1
    .\Initialize-DevDatabase.ps1 -DryRun
#>
param(
    [string]$InstanceName = 'HRMSDev',
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
if (-not (Get-Command sqllocaldb -ErrorAction SilentlyContinue)) {
    throw 'SqlLocalDB not found. Install SQL Server Express LocalDB, then run this again.'
}

$existing = sqllocaldb info | Where-Object { $_.Trim() -eq $InstanceName }
if (-not $existing) { sqllocaldb create $InstanceName | Out-Null }
sqllocaldb start $InstanceName | Out-Null

$repo = Resolve-Path (Join-Path $PSScriptRoot '..\..')
$arguments = @('--server', "(localdb)\$InstanceName", '--trust-cert', '--create-db',
               '--scripts', (Join-Path $repo 'database'))
if ($DryRun) { $arguments += '--dry-run' }

dotnet run --project (Join-Path $repo 'src\Hrms.Migrator') -- @arguments
if ($LASTEXITCODE -ne 0) { throw "Migrator failed with exit code $LASTEXITCODE." }
Write-Host "Dev databases ready on (localdb)\$InstanceName"
