Imports System.Security.Cryptography
Imports System.Text

''' <summary>SHA-256 of the script text. Line endings and a BOM are ignored so every PC gets the same value.</summary>
Public NotInheritable Class ScriptChecksum
    Private Const BOM As Char = ChrW(&HFEFF)

    Private Sub New()
    End Sub

    Public Shared Function Compute(text As String) As String
        Dim normalized As String = text.TrimStart(BOM).Replace(vbCrLf, vbLf).Replace(vbCr, vbLf)
        Dim hash As Byte() = SHA256.HashData(Encoding.UTF8.GetBytes(normalized))
        Return Convert.ToHexString(hash).ToLowerInvariant()
    End Function
End Class
