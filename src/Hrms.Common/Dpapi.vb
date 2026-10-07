Option Strict On
Imports System.IO
Imports System.Security.Cryptography
Imports System.Text.Json
Imports System.Security.Cryptography.ProtectedData

''' <summary>Machine-scope DPAPI helpers, key store and encrypted connection-string store. Windows only.</summary>
Public Module Dpapi
    Public Function Protect(data As Byte()) As Byte()
        Return ProtectedData.Protect(data, Nothing, DataProtectionScope.LocalMachine)
    End Function

    Public Function Unprotect(data As Byte()) As Byte()
        Return ProtectedData.Unprotect(data, Nothing, DataProtectionScope.LocalMachine)
    End Function
End Module

Public NotInheritable Class KeyStore
    Private Shared ReadOnly KeyFile As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "HRMS", "keys", "data.key")

    ''' <summary>AES-256 key for field encryption. Created on first use. Back it up separately from the server.</summary>
    Public Shared Function GetDataKey() As Byte()
        If File.Exists(KeyFile) Then Return Dpapi.Unprotect(File.ReadAllBytes(KeyFile))
        Directory.CreateDirectory(Path.GetDirectoryName(KeyFile))
        Dim key = RandomNumberGenerator.GetBytes(32)
        File.WriteAllBytes(KeyFile, Dpapi.Protect(key))
        Return key
    End Function
End Class

Public Class ConnectionStrings
    Public Property Master As String = ""
    Public Property Data As String = ""
End Class

Public NotInheritable Class ConnectionStringStore
    Private Shared ReadOnly FilePath As String =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "HRMS", "config", "connection.dat")

    ''' <summary>Called by the installer / setup tool. Stores both strings DPAPI-encrypted.</summary>
    Public Shared Sub Save(cs As ConnectionStrings)
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath))
        Dim json = JsonSerializer.SerializeToUtf8Bytes(cs)
        File.WriteAllBytes(FilePath, Dpapi.Protect(json))
    End Sub

    Public Shared Function Load() As ConnectionStrings
        If File.Exists(FilePath) Then
            Dim json = Dpapi.Unprotect(File.ReadAllBytes(FilePath))
            Return JsonSerializer.Deserialize(Of ConnectionStrings)(json)
        End If
#If DEBUG Then
        Dim m = Environment.GetEnvironmentVariable("HRMS_CONN_MASTER")
        Dim d = Environment.GetEnvironmentVariable("HRMS_CONN_DATA")
        If Not String.IsNullOrEmpty(m) AndAlso Not String.IsNullOrEmpty(d) Then
            Return New ConnectionStrings With {.Master = m, .Data = d}
        End If
#End If
        Throw New FileNotFoundException("Database connection is not configured. Run setup.")
    End Function
End Class
