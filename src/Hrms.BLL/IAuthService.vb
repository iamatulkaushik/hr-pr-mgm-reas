Option Strict On
Imports Hrms.Models

''' <summary>Contract for the UI (Dev Sigma codes against this). UI hides, BLL enforces.</summary>
Public Interface IAuthService
    Function Login(username As String, password As String) As AuthResult
    Sub Logout(session As SessionInfo)
    Function ChangePassword(session As SessionInfo, oldPassword As String, newPassword As String) As OperationResult
    Function SelectCompany(session As SessionInfo, companyId As Integer) As OperationResult
    ''' <summary>UI helper. Cheap, no DB hit.</summary>
    Function CanAccess(session As SessionInfo, moduleName As String, action As String) As Boolean
    ''' <summary>BLL guard. Throws PermissionDeniedException; also re-checks company access in the DB.</summary>
    Sub Demand(session As SessionInfo, permissionCode As String)
    Function NeedsBootstrap() As Boolean
    Function BootstrapFirstAssociate(username As String, fullName As String, password As String) As OperationResult
End Interface
