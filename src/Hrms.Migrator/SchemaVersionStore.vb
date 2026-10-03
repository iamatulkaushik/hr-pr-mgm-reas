Imports Microsoft.Data.SqlClient

''' <summary>cfg.SchemaVersion: which numbered scripts a database has had applied.</summary>
Public NotInheritable Class SchemaVersionStore
    Private Const COMMAND_TIMEOUT_SECONDS As Integer = 60

    Private Const ENSURE_SCHEMA_SQL As String = "IF SCHEMA_ID('cfg') IS NULL EXEC('CREATE SCHEMA cfg');"

    Private Const ENSURE_TABLE_SQL As String =
        "IF OBJECT_ID('cfg.SchemaVersion') IS NULL" & vbLf &
        "CREATE TABLE cfg.SchemaVersion (" & vbLf &
        "    SchemaVersionID INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchemaVersion PRIMARY KEY," & vbLf &
        "    ScriptVersion INT NOT NULL CONSTRAINT UQ_SchemaVersion_ScriptVersion UNIQUE," & vbLf &
        "    FileName NVARCHAR(200) NOT NULL," & vbLf &
        "    Checksum CHAR(64) NOT NULL," & vbLf &
        "    AppliedOn DATETIME2 NOT NULL CONSTRAINT DF_SchemaVersion_AppliedOn DEFAULT SYSDATETIME()," & vbLf &
        "    AppliedBy NVARCHAR(128) NOT NULL CONSTRAINT DF_SchemaVersion_AppliedBy DEFAULT SUSER_SNAME()," & vbLf &
        "    ElapsedMs INT NOT NULL" & vbLf &
        ");"

    Private Const SELECT_SQL As String =
        "SELECT ScriptVersion, FileName, Checksum FROM cfg.SchemaVersion ORDER BY ScriptVersion;"

    Private Const INSERT_SQL As String =
        "INSERT INTO cfg.SchemaVersion (ScriptVersion, FileName, Checksum, ElapsedMs) VALUES (@Version, @FileName, @Checksum, @ElapsedMs);"

    Private Sub New()
    End Sub

    Public Shared Sub EnsureTable(cn As SqlConnection)
        RunNonQuery(cn, ENSURE_SCHEMA_SQL)
        RunNonQuery(cn, ENSURE_TABLE_SQL)
    End Sub

    Private Shared Sub RunNonQuery(cn As SqlConnection, sql As String)
        Using cmd As New SqlCommand(sql, cn) With {.CommandTimeout = COMMAND_TIMEOUT_SECONDS}
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    Public Shared Function GetApplied(cn As SqlConnection) As List(Of AppliedMigration)
        Dim list As New List(Of AppliedMigration)
        Using cmd As New SqlCommand(SELECT_SQL, cn) With {.CommandTimeout = COMMAND_TIMEOUT_SECONDS}
            Using reader As SqlDataReader = cmd.ExecuteReader()
                While reader.Read()
                    list.Add(New AppliedMigration With {
                        .Version = reader.GetInt32(0), .FileName = reader.GetString(1), .Checksum = reader.GetString(2)})
                End While
            End Using
        End Using
        Return list
    End Function

    Public Shared Sub Record(cn As SqlConnection, tx As SqlTransaction, script As MigrationScript, elapsedMs As Integer)
        Using cmd As New SqlCommand(INSERT_SQL, cn, tx) With {.CommandTimeout = COMMAND_TIMEOUT_SECONDS}
            cmd.Parameters.AddWithValue("@Version", script.Version)
            cmd.Parameters.AddWithValue("@FileName", script.FileName)
            cmd.Parameters.AddWithValue("@Checksum", script.Checksum)
            cmd.Parameters.AddWithValue("@ElapsedMs", elapsedMs)
            cmd.ExecuteNonQuery()
        End Using
    End Sub
End Class
