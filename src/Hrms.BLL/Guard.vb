Public NotInheritable Class Guard

    Private Sub New()
    End Sub

    Public Shared Sub Require(user As IUserContext, moduleName As String, action As String)
        If Not user.CanAccess(moduleName, action) Then
            Throw New AccessDeniedException(moduleName, action)
        End If
    End Sub

End Class
