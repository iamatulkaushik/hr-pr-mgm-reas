Imports Microsoft.Data.SqlClient

Public NotInheritable Class ConnectionFactory
    Public Const APPLICATION_NAME As String = "Hrms.Migrator"
    Public Const SYSTEM_DATABASE As String = "master"
    Private Const CONNECT_TIMEOUT_SECONDS As Integer = 30

    Private Sub New()
    End Sub

    Public Shared Function Open(options As MigratorOptions, database As String) As SqlConnection
        Dim cn As New SqlConnection(Build(options, database))
        cn.Open()
        Return cn
    End Function

    Public Shared Function Build(options As MigratorOptions, database As String) As String
        Dim builder As New SqlConnectionStringBuilder() With {
            .DataSource = options.Server,
            .InitialCatalog = database,
            .ApplicationName = APPLICATION_NAME,
            .ConnectTimeout = CONNECT_TIMEOUT_SECONDS,
            .TrustServerCertificate = options.TrustServerCertificate}
        If options.UserName.Length > 0 Then
            builder.UserID = options.UserName
            builder.Password = options.Password
        Else
            builder.IntegratedSecurity = True
        End If
        Return builder.ConnectionString
    End Function
End Class
