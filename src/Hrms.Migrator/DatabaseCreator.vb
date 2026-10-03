Imports Microsoft.Data.SqlClient

Public NotInheritable Class DatabaseCreator
    Private Const CREATE_SQL As String =
        "IF DB_ID(@Name) IS NULL BEGIN DECLARE @sql NVARCHAR(400) = N'CREATE DATABASE ' + QUOTENAME(@Name); EXEC (@sql); SELECT 1; END ELSE SELECT 0;"

    Private Sub New()
    End Sub

    ''' <summary>Creates the database when missing. Returns True if it was created.</summary>
    Public Shared Function EnsureExists(options As MigratorOptions, database As String) As Boolean
        SqlNames.EnsureSafe(database)
        Using cn As SqlConnection = ConnectionFactory.Open(options, ConnectionFactory.SYSTEM_DATABASE)
            Using cmd As New SqlCommand(CREATE_SQL, cn)
                cmd.Parameters.AddWithValue("@Name", database)
                Return Convert.ToInt32(cmd.ExecuteScalar()) = 1
            End Using
        End Using
    End Function
End Class
