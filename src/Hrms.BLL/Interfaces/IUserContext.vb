Public Interface IUserContext

    ReadOnly Property UserId As Integer
    ReadOnly Property UserName As String
    ReadOnly Property UserType As String
    ReadOnly Property CompanyId As Integer
    ReadOnly Property CompanyName As String

    Function CanAccess(moduleName As String, action As String) As Boolean

End Interface
