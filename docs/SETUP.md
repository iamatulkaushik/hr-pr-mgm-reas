# Setup

## Layout

| Folder | What |
|---|---|
| `src/Hrms.Common` | crypto, logging, config, helpers (no dependencies) |
| `src/Hrms.Models` | DTOs and entities |
| `src/Hrms.DAL` | ADO.NET, stored procedure calls |
| `src/Hrms.BLL` | business rules and calculations (calls DAL) |
| `src/Hrms.Reports` | Crystal reports and report engine (calls DAL) |
| `src/Hrms.UI` | WinForms (calls BLL only, never DAL) |
| `src/Hrms.Installer` | holds `installer/` so it shows in the solution; Inno Setup builds the setup |
| `src/Hrms.Migrator` | applies numbered SQL scripts |
| `database/HRMS_Master/migrations`, `database/HRMS_Data/migrations` | `NNNN_name.sql` scripts, one folder per database |

Reference rule: UI -> BLL -> DAL -> Models -> Common. UI never references DAL.

## Developer PC (LocalDB)

1. Install .NET 10 SDK and SQL Server Express LocalDB.
2. `scripts\dev\Initialize-DevDatabase.ps1` creates `(localdb)\HRMSDev`, both databases, and applies every script.
3. Run it again after pulling new scripts. `-DryRun` lists what is pending.

## Server PC (SQL Server Express)

1. Install SQL Server Express, named instance `HRMS`, mixed mode, your Windows admin as sysadmin.
2. Run as Administrator:
   `scripts\server\Set-SqlServerHardening.ps1 -Port 14330 -ClientIps 192.168.1.21,192.168.1.22`
   It sets a fixed non-default port, disables `sa`, disables SQL Browser, and opens the firewall for the listed PCs only.
3. Create the databases and apply scripts (set the password in the environment, not on the command line):
   ```
   set HRMS_SQL_PASSWORD=...
   Hrms.Migrator --server "localhost,14330" --user hrms_admin --create-db --backup-dir D:\HRMS\Backup
   ```
   `--backup-dir` is a folder on the server PC. A backup runs before scripts are applied to an existing database.

## Migration rules

- Name: four digits, underscore, name: `0012_att_leave.sql`. Gaps are fine.
- Idempotent (`IF NOT EXISTS`, `CREATE OR ALTER`). Batches separated by a line holding only `GO`.
- Never edit a merged script. The migrator stops if an applied script's checksum changed. Add a new number.
- A new script must be numbered above the latest applied one, or the migrator stops.
- Each script runs in one transaction and is recorded in `cfg.SchemaVersion`.
- Do not write a line of only `GO` inside a comment or string.
- Never commit `.mdf`, `.ldf` or `.bak` files.
