Public Class AccessDeniedException
    Inherits BusinessRuleException

    Public Sub New(moduleName As String, action As String)
        MyBase.New($"You do not have permission for {moduleName} ({action}).")
    End Sub

End Class
