Imports System.Text

''' <summary>
''' Splits a script on lines that contain only GO (any case). Empty batches are dropped.
''' Limit: a line of just GO inside a multi-line comment or string would also split. Do not write that.
''' </summary>
Public NotInheritable Class SqlBatchSplitter
    Private Const SEPARATOR As String = "GO"

    Private Sub New()
    End Sub

    Public Shared Function Split(script As String) As List(Of String)
        Dim batches As New List(Of String)
        Dim current As New StringBuilder()
        For Each line As String In script.Replace(vbCrLf, vbLf).Split(ControlChars.Lf)
            If String.Equals(line.Trim(), SEPARATOR, StringComparison.OrdinalIgnoreCase) Then
                Flush(current, batches)
            Else
                current.Append(line).Append(vbLf)
            End If
        Next
        Flush(current, batches)
        Return batches
    End Function

    Private Shared Sub Flush(current As StringBuilder, batches As List(Of String))
        Dim text As String = current.ToString().Trim()
        If text.Length > 0 Then batches.Add(text)
        current.Clear()
    End Sub
End Class
