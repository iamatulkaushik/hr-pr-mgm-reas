Imports System.IO
Imports Microsoft.Data.SqlClient

''' <summary>
''' Full backup before an upgrade. The folder is on the SQL Server PC, because SQL Server writes the file.
''' Express has no backup compression or encryption; the nightly encrypted job is separate.
''' </summary>
Public NotInheritable Class DatabaseBackup
    Private Const BACKUP_TIMEOUT_SECONDS As Integer = 900

    Private Sub New()
    End Sub

    Public Shared Function Run(cn As SqlConnection, database As String, directory As String, firstPendingVersion As Integer) As String
        SqlNames.EnsureSafe(database)
        Dim fileName As String = $"{database}_pre-{firstPendingVersion:0000}_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
        Dim fullPath As String = Path.Combine(directory, fileName)
        Dim sql As String = $"BACKUP DATABASE [{database}] TO DISK = @Path WITH INIT, CHECKSUM;"
        Using cmd As New SqlCommand(sql, cn) With {.CommandTimeout = BACKUP_TIMEOUT_SECONDS}
            cmd.Parameters.AddWithValue("@Path", fullPath)
            cmd.ExecuteNonQuery()
        End Using
        Return fullPath
    End Function
End Class
