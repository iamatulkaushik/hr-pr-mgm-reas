Imports System.IO
Imports System.Text.Json

''' <summary>
''' Settings read from appsettings.json next to the exe.
''' The file is not committed. Copy appsettings.example.json and edit it.
''' </summary>
Public NotInheritable Class AppConfig

    Public Const FILE_NAME As String = "appsettings.json"

    Public Property ConnectionString As String = String.Empty
    Public Property LogFolder As String = String.Empty

    ''' <summary>Server name for the status bar. Never includes a user name or password.</summary>
    Public ReadOnly Property ServerDisplayName As String
        Get
            Return ReadServerName(ConnectionString)
        End Get
    End Property

    Public Shared Function Load(path As String) As AppConfig
        If Not File.Exists(path) Then
            Throw New FileNotFoundException(
                "appsettings.json was not found. Copy appsettings.example.json to appsettings.json and edit it.", path)
        End If

        Using stream = File.OpenRead(path)
            Using doc = JsonDocument.Parse(stream)
                Dim root = doc.RootElement
                Dim config As New AppConfig()
                config.ConnectionString = ReadString(root, "ConnectionString")
                Dim logFolder = ReadString(root, "LogFolder")
                config.LogFolder = If(String.IsNullOrWhiteSpace(logFolder),
                                      DefaultLogFolder(),
                                      Environment.ExpandEnvironmentVariables(logFolder))
                Return config
            End Using
        End Using
    End Function

    Public Shared Function DefaultLogFolder() As String
        Dim programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData)
        Return Path.Combine(programData, "RevolutionHRMS", "Logs")
    End Function

    Private Shared Function ReadString(root As JsonElement, name As String) As String
        Dim value As JsonElement
        If root.TryGetProperty(name, value) AndAlso value.ValueKind = JsonValueKind.String Then
            Return If(value.GetString(), String.Empty)
        End If
        Return String.Empty
    End Function

    Private Shared Function ReadServerName(connectionString As String) As String
        If String.IsNullOrWhiteSpace(connectionString) Then Return "-"
        Dim builder As New System.Data.Common.DbConnectionStringBuilder()
        Try
            builder.ConnectionString = connectionString
        Catch ex As ArgumentException
            Return "-"
        End Try
        For Each key In {"Server", "Data Source", "Address", "Addr", "Network Address"}
            Dim value As Object = Nothing
            If builder.TryGetValue(key, value) AndAlso value IsNot Nothing Then
                Return value.ToString()
            End If
        Next
        Return "-"
    End Function

End Class
