Imports System.Linq

Public NotInheritable Class OptionsParser
    Public Const ENV_SERVER As String = "HRMS_SQL_SERVER"
    Public Const ENV_PASSWORD As String = "HRMS_SQL_PASSWORD"

    Public Shared ReadOnly UsageText As String =
        "Hrms.Migrator - applies numbered SQL scripts to HRMS_Master and HRMS_Data." & Environment.NewLine &
        "  --server <name>           SQL Server (or env HRMS_SQL_SERVER). Example: (localdb)\HRMSDev" & Environment.NewLine &
        "  --user <login>            SQL login. Password comes from env HRMS_SQL_PASSWORD. Omit for Windows login." & Environment.NewLine &
        "  --master-db <name>        Default HRMS_Master" & Environment.NewLine &
        "  --data-db <name>          Default HRMS_Data" & Environment.NewLine &
        "  --scripts <folder>        Folder holding HRMS_Master\migrations and HRMS_Data\migrations. Default .\database" & Environment.NewLine &
        "  --target <both|master|data>" & Environment.NewLine &
        "  --create-db               Create the databases if missing" & Environment.NewLine &
        "  --backup-dir <folder>     Folder on the SQL Server PC. Backs up before applying to an existing database" & Environment.NewLine &
        "  --trust-cert              Trust the server certificate (LocalDB, no certificate)" & Environment.NewLine &
        "  --dry-run                 Show pending scripts, change nothing" & Environment.NewLine &
        "  --help"

    Private Sub New()
    End Sub

    Public Shared Function Parse(args As String(), getEnv As Func(Of String, String)) As MigratorOptions
        Dim options As New MigratorOptions()
        options.Server = If(getEnv(ENV_SERVER), "")
        options.Password = If(getEnv(ENV_PASSWORD), "")

        Dim index As Integer = 0
        While index < args.Length
            index = ApplyArgument(options, args, index)
        End While
        Validate(options)
        Return options
    End Function

    ''' <summary>Applies one argument and returns the index of the next one.</summary>
    Private Shared Function ApplyArgument(options As MigratorOptions, args As String(), index As Integer) As Integer
        Select Case args(index).ToLowerInvariant()
            Case "--create-db"
                options.CreateDatabases = True
            Case "--dry-run"
                options.DryRun = True
            Case "--trust-cert"
                options.TrustServerCertificate = True
            Case "--server"
                options.Server = ReadValue(args, index)
                Return index + 2
            Case "--user"
                options.UserName = ReadValue(args, index)
                Return index + 2
            Case "--master-db"
                options.MasterDatabase = ReadValue(args, index)
                Return index + 2
            Case "--data-db"
                options.DataDatabase = ReadValue(args, index)
                Return index + 2
            Case "--scripts"
                options.ScriptsRoot = ReadValue(args, index)
                Return index + 2
            Case "--backup-dir"
                options.BackupDirectory = ReadValue(args, index)
                Return index + 2
            Case "--target"
                options.Target = ParseTarget(ReadValue(args, index))
                Return index + 2
            Case Else
                Throw New UsageException($"Unknown option '{args(index)}'.")
        End Select
        Return index + 1
    End Function

    Private Shared Function ReadValue(args As String(), index As Integer) As String
        If index + 1 >= args.Length OrElse args(index + 1).StartsWith("--", StringComparison.Ordinal) Then
            Throw New UsageException($"Option '{args(index)}' needs a value.")
        End If
        Return args(index + 1)
    End Function

    Private Shared Function ParseTarget(value As String) As MigrationTarget
        Select Case value.ToLowerInvariant()
            Case "both"
                Return MigrationTarget.Both
            Case "master"
                Return MigrationTarget.Master
            Case "data"
                Return MigrationTarget.Data
            Case Else
                Throw New UsageException($"Unknown target '{value}'. Use both, master or data.")
        End Select
    End Function

    Private Shared Sub Validate(options As MigratorOptions)
        If options.Server.Trim().Length = 0 Then
            Throw New UsageException($"Server is required. Use --server or set {ENV_SERVER}.")
        End If
        If options.UserName.Length > 0 AndAlso options.Password.Length = 0 Then
            Throw New UsageException($"A SQL login needs its password in the {ENV_PASSWORD} environment variable.")
        End If
        SqlNames.EnsureSafe(options.MasterDatabase)
        SqlNames.EnsureSafe(options.DataDatabase)
    End Sub
End Class
