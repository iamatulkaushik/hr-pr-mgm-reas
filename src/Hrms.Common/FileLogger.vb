Imports System.Globalization
Imports System.IO

''' <summary>
''' Writes one log file per day (yyyy-MM-dd.log). Never log passwords, Aadhaar, PAN or bank numbers.
''' Logging must never crash the app, so file errors are ignored here.
''' </summary>
Public NotInheritable Class FileLogger

    Private ReadOnly _folder As String
    Private ReadOnly _sync As New Object()

    Public Sub New(folder As String)
        _folder = folder
    End Sub

    Public Sub Info(message As String)
        Write("INFO", message)
    End Sub

    Public Sub Warn(message As String)
        Write("WARN", message)
    End Sub

    ''' <summary>Logs the exception and returns a short reference to show to the user.</summary>
    Public Function LogError(context As String, ex As Exception) As String
        Dim reference = Guid.NewGuid().ToString("N").Substring(0, 8)
        Write("ERROR", $"[{reference}] {context}{Environment.NewLine}{ex}")
        Return reference
    End Function

    Private Sub Write(level As String, message As String)
        Dim stamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
        Dim fileName = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) & ".log"
        Dim line = $"{stamp} [{level}] {message}{Environment.NewLine}"
        Try
            SyncLock _sync
                Directory.CreateDirectory(_folder)
                File.AppendAllText(Path.Combine(_folder, fileName), line)
            End SyncLock
        Catch ex As IOException
            ' A failed log write must not stop the app.
        Catch ex As UnauthorizedAccessException
            ' A failed log write must not stop the app.
        End Try
    End Sub

End Class
