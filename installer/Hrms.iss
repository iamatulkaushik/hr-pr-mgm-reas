; Inno Setup script for Revolution HRMS. Placeholder owned by Dev Brave; grows as modules land.
; Build: iscc installer\Hrms.iss   (publish the UI and the migrator first, see docs/SETUP.md)
#define AppName "Revolution HRMS"
#define AppVersion "0.1.0"
#define UiPublish "..\src\Hrms.UI\bin\Release\net10.0-windows\publish"
#define MigratorPublish "..\src\Hrms.Migrator\bin\Release\net10.0\publish"

[Setup]
AppName={#AppName}
AppVersion={#AppVersion}
DefaultDirName={autopf}\RevolutionHRMS
DefaultGroupName={#AppName}
OutputDir=Output
OutputBaseFilename=RevolutionHRMS-Setup-{#AppVersion}
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin
Compression=lzma2
SolidCompression=yes

[Files]
Source: "{#UiPublish}\*"; DestDir: "{app}"; Flags: recursesubdirs ignoreversion
Source: "{#MigratorPublish}\*"; DestDir: "{app}\Migrator"; Flags: recursesubdirs ignoreversion
Source: "..\database\*"; DestDir: "{app}\database"; Flags: recursesubdirs ignoreversion

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\Hrms.UI.exe"

; TODO (later phases): .NET 10 Desktop Runtime check, Crystal runtime, pre-upgrade backup and migrator call.
