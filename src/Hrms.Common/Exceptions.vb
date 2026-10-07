Option Strict On

''' <summary>Expected rule failure. Message is safe to show the user.</summary>
Public Class BusinessRuleException
    Inherits Exception

    Public Sub New(message As String)
        MyBase.New(message)
    End Sub

    Public Sub New(message As String, inner As Exception)
        MyBase.New(message, inner)
    End Sub
End Class

Public Class PermissionDeniedException
    Inherits Exception

    Public Sub New(permissionCode As String)
        MyBase.New("You do not have permission: " & permissionCode)
    End Sub
End Class
