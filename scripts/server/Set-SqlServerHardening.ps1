<#
.SYNOPSIS
    One-time hardening of the HRMS SQL Server Express instance on the server PC. Run as Administrator.
.DESCRIPTION
    - Fixed, non-default TCP port (dynamic ports off)
    - sa login disabled
    - SQL Browser service stopped and disabled
    - Windows Firewall rule: that port, only from the client PCs you list
    Install SQL Server Express first (named instance, mixed mode) and make sure the account
    running this script is a SQL sysadmin. See docs/SETUP.md.
.EXAMPLE
    .\Set-SqlServerHardening.ps1 -Port 14330 -ClientIps 192.168.1.21,192.168.1.22,192.168.1.23
#>
param(
    [string]$InstanceName = 'HRMS',
    [Parameter(Mandatory)][ValidateRange(1025, 65535)][int]$Port,
    [Parameter(Mandatory)][string[]]$ClientIps
)

$ErrorActionPreference = 'Stop'
if ($Port -eq 1433) { throw 'Use a non-default port. 1433 is the default.' }

$sqlRoot = 'HKLM:\SOFTWARE\Microsoft\Microsoft SQL Server'
$instanceId = (Get-ItemProperty "$sqlRoot\Instance Names\SQL").$InstanceName
if (-not $instanceId) { throw "SQL Server instance '$InstanceName' not found on this PC." }

# 1. TCP/IP on, fixed port, no dynamic port
$tcp = "$sqlRoot\$instanceId\MSSQLServer\SuperSocketNetLib\Tcp"
Set-ItemProperty -Path $tcp -Name Enabled -Value 1
Set-ItemProperty -Path "$tcp\IPAll" -Name TcpPort -Value "$Port"
Set-ItemProperty -Path "$tcp\IPAll" -Name TcpDynamicPorts -Value ''

# 2. Restart the instance so the port takes effect
$service = 'MSSQL$' + $InstanceName
Restart-Service -Name $service -Force
Write-Host "Instance $InstanceName now listens on TCP $Port."

# 3. Disable sa
sqlcmd -S "tcp:localhost,$Port" -E -b -Q "ALTER LOGIN sa DISABLE;"
if ($LASTEXITCODE -ne 0) { throw 'Could not disable sa. Run as a SQL sysadmin and check that sqlcmd is installed.' }

# 4. SQL Browser off (clients connect by port)
$browser = Get-Service -Name SQLBrowser -ErrorAction SilentlyContinue
if ($browser) {
    Stop-Service -Name SQLBrowser -Force -ErrorAction SilentlyContinue
    Set-Service -Name SQLBrowser -StartupType Disabled
}

# 5. Firewall: only the listed client PCs
$ruleName = "HRMS SQL Server $Port"
Get-NetFirewallRule -DisplayName $ruleName -ErrorAction SilentlyContinue | Remove-NetFirewallRule
New-NetFirewallRule -DisplayName $ruleName -Direction Inbound -Protocol TCP -LocalPort $Port `
    -RemoteAddress $ClientIps -Action Allow -Profile Domain, Private | Out-Null

# 6. Check
$isDisabled = sqlcmd -S "tcp:localhost,$Port" -E -h -1 -W -Q "SET NOCOUNT ON; SELECT is_disabled FROM sys.server_principals WHERE name = 'sa';"
Write-Host "sa disabled: $($isDisabled.Trim())   (1 = yes)"
Write-Host "Clients connect with:  Server=<this PC>,$Port"
