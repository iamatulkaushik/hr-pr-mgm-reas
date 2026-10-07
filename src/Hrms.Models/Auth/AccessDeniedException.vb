''' <summary>Thrown when a user does something their permissions or companies do not allow.</summary>
Public Class AccessDeniedException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

End Class
