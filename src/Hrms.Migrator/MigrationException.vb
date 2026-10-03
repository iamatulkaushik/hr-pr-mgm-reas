''' <summary>A migration problem the operator must fix (bad name, edited script, out-of-order script).</summary>
Public Class MigrationException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub
End Class
