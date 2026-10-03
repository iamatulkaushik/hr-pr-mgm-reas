Imports System.Text.RegularExpressions

''' <summary>Database names go into CREATE DATABASE and BACKUP text, so only plain names are allowed.</summary>
Public NotInheritable Class SqlNames
    Private Shared ReadOnly _safeName As New Regex("^[A-Za-z0-9_]{1,100}$", RegexOptions.Compiled)

    Private Sub New()
    End Sub

    Public Shared Sub EnsureSafe(name As String)
        If name Is Nothing OrElse Not _safeName.IsMatch(name) Then
            Throw New UsageException($"'{name}' is not a valid database name. Use letters, digits and underscore.")
        End If
    End Sub
End Class
