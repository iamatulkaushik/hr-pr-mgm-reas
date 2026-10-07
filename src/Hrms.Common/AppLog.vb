Option Strict On
Imports System.IO

''' <summary>Rolling daily log in %ProgramData%\HRMS\Logs. Never pass passwords, hashes or personal data.</summary>
Public NotInheritable Class AppLog
    Private Shared ReadOnly Sync As New Object()
    Private Shared ReadOnly Dir As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "HRMS", "Logs")

    Public Shared Sub Info(message As String)
        Write("INFO", message)
    End Sub

    Public Shared Sub [Error](message As String, Optional ex As Exception = Nothing)
        Write("ERROR", If(ex Is Nothing, message, message & " | " & ex.ToString()))
    End Sub

    Private Shared Sub Write(level As String, message As String)
        Try
            SyncLock Sync
                Directory.CreateDirectory(Dir)
                Dim logFile = Path.Combine(Dir, $"hrms-{Date.Now:yyyyMMdd}.log")
                File.AppendAllText(logFile, $"{Date.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}")
            End SyncLock
        Catch
            ' logging must never crash the app
        End Try
    End Sub
End Class
