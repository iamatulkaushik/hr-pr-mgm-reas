Imports System.Diagnostics
Imports System.IO
Imports Microsoft.Data.SqlClient

''' <summary>Applies pending scripts to HRMS_Master and HRMS_Data, each script in one transaction.</summary>
Public Class MigrationRunner
    Private Const MASTER_FOLDER As String = "HRMS_Master"
    Private Const DATA_FOLDER As String = "HRMS_Data"
    Private Const MIGRATIONS_FOLDER As String = "migrations"
    Private Const SCRIPT_TIMEOUT_SECONDS As Integer = 300

    Private ReadOnly _options As MigratorOptions
    Private ReadOnly _log As Action(Of String)

    Public Sub New(options As MigratorOptions, log As Action(Of String))
        _options = options
        _log = log
    End Sub

    Public Sub Run()
        If _options.Target <> MigrationTarget.Data Then MigrateDatabase(_options.MasterDatabase, MASTER_FOLDER)
        If _options.Target <> MigrationTarget.Master Then MigrateDatabase(_options.DataDatabase, DATA_FOLDER)
    End Sub

    Private Sub MigrateDatabase(database As String, folderName As String)
        _log($"== {database} ==")
        Dim scripts As List(Of MigrationScript) = ScriptLoader.Load(Path.Combine(_options.ScriptsRoot, folderName, MIGRATIONS_FOLDER))
        If _options.CreateDatabases AndAlso Not _options.DryRun AndAlso DatabaseCreator.EnsureExists(_options, database) Then
            _log("Created database.")
        End If

        Using cn As SqlConnection = ConnectionFactory.Open(_options, database)
            If Not _options.DryRun Then SchemaVersionStore.EnsureTable(cn)
            Dim plan As MigrationPlan = MigrationPlanner.Plan(scripts, ReadApplied(cn))
            If plan.Pending.Count = 0 Then
                _log("Up to date.")
                Return
            End If
            ApplyPlan(cn, database, plan)
        End Using
    End Sub

    ''' <summary>In a dry run the version table may not exist yet, which means nothing is applied.</summary>
    Private Function ReadApplied(cn As SqlConnection) As List(Of AppliedMigration)
        If _options.DryRun AndAlso Not TableExists(cn) Then Return New List(Of AppliedMigration)
        Return SchemaVersionStore.GetApplied(cn)
    End Function

    Private Shared Function TableExists(cn As SqlConnection) As Boolean
        Using cmd As New SqlCommand("SELECT CASE WHEN OBJECT_ID('cfg.SchemaVersion') IS NULL THEN 0 ELSE 1 END;", cn)
            Return Convert.ToInt32(cmd.ExecuteScalar()) = 1
        End Using
    End Function

    Private Sub ApplyPlan(cn As SqlConnection, database As String, plan As MigrationPlan)
        For Each script As MigrationScript In plan.Pending
            _log($"{If(_options.DryRun, "Pending", "Applying")}: {script.FileName}")
        Next
        If _options.DryRun Then Return

        If Not plan.IsFirstRun AndAlso _options.BackupDirectory.Length > 0 Then
            _log("Backup: " & DatabaseBackup.Run(cn, database, _options.BackupDirectory, plan.Pending(0).Version))
        End If
        For Each script As MigrationScript In plan.Pending
            ApplyScript(cn, script)
        Next
        _log($"Applied {plan.Pending.Count} script(s).")
    End Sub

    Private Sub ApplyScript(cn As SqlConnection, script As MigrationScript)
        Dim timer As Stopwatch = Stopwatch.StartNew()
        Using tx As SqlTransaction = cn.BeginTransaction()
            Try
                For Each batch As String In SqlBatchSplitter.Split(script.Sql)
                    Using cmd As New SqlCommand(batch, cn, tx) With {.CommandTimeout = SCRIPT_TIMEOUT_SECONDS}
                        cmd.ExecuteNonQuery()
                    End Using
                Next
                SchemaVersionStore.Record(cn, tx, script, CInt(timer.ElapsedMilliseconds))
                tx.Commit()
            Catch ex As SqlException
                tx.Rollback()
                Throw New MigrationException($"{script.FileName} failed and was rolled back: {ex.Message}")
            End Try
        End Using
        _log($"  done in {timer.ElapsedMilliseconds} ms")
    End Sub
End Class
