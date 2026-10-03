Public Class MigratorOptions
    Public Const DEFAULT_MASTER_DATABASE As String = "HRMS_Master"
    Public Const DEFAULT_DATA_DATABASE As String = "HRMS_Data"
    Public Const DEFAULT_SCRIPTS_ROOT As String = "database"

    Public Property Server As String = ""
    Public Property UserName As String = ""
    ''' <summary>Read from HRMS_SQL_PASSWORD only. Never taken from the command line.</summary>
    Public Property Password As String = ""
    Public Property MasterDatabase As String = DEFAULT_MASTER_DATABASE
    Public Property DataDatabase As String = DEFAULT_DATA_DATABASE
    Public Property ScriptsRoot As String = DEFAULT_SCRIPTS_ROOT
    Public Property BackupDirectory As String = ""
    Public Property CreateDatabases As Boolean
    Public Property DryRun As Boolean
    Public Property TrustServerCertificate As Boolean
    Public Property Target As MigrationTarget = MigrationTarget.Both
End Class
