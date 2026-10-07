@echo off
REM Usage: apply_phase1.bat <server\instance,port> <AppLoginPassword>
REM Needs sqlcmd and Windows auth with CREATE DATABASE / CREATE LOGIN rights.
set SRV=%1
set APPPWD=%2
if "%SRV%"=="" (echo Usage: apply_phase1.bat server,port AppPwd & exit /b 1)
sqlcmd -S %SRV% -E -b -Q "IF DB_ID('HRMS_Master') IS NULL CREATE DATABASE HRMS_Master; IF DB_ID('HRMS_Data') IS NULL CREATE DATABASE HRMS_Data;" || exit /b 1
for %%F in (0001_init 0002_seed 0003_procs) do sqlcmd -S %SRV% -E -b -d HRMS_Master -i Master\%%F.sql || exit /b 1
sqlcmd -S %SRV% -E -b -d HRMS_Master -v AppLoginPassword="%APPPWD%" -i Master\0004_security.sql || exit /b 1
for %%F in (0001_init 0002_procs 0003_security) do sqlcmd -S %SRV% -E -b -d HRMS_Data -i Data\%%F.sql || exit /b 1
echo Phase 1 database applied.
