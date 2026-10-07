Imports Hrms.Models
Imports Hrms.Models.Auth
Imports Hrms.Models.Company


''' <summary>
''' Agreed BLL contract for authentication. Implementation: Dev Brave (AuthService over sec.usp_* procedures).
''' Dev Sigma's forms code only against this interface.
''' </summary>
Public Interface IAuthService

    ''' <summary>Checks user name and password. Counts failures, locks the account after 5 (Security.MaxFailedLogins).</summary>
    Function LoginResult(userName As String, password As String) As LoginResult

    ''' <summary>Re-checks the password for the idle-lock screen. Same lockout rules as Login.</summary>
    Function Unlock(userName As String, password As String) As LoginResult

    Function ChangePassword(userId As Integer, oldPassword As String, newPassword As String) As OperationResult

    ''' <summary>Companies in sec.UserCompany for the user (exactly one for a Company user).</summary>
    Function GetAssignedCompanies(userId As Integer) As IReadOnlyList(Of CompanyInfo)

    ''' <summary>Verifies assignment and makes it the active company for SESSION_CONTEXT('CompanyID').</summary>
    Function SelectCompany(userId As Integer, companyId As Integer) As OperationResult

    ''' <summary>BLL permission check (the UI only hides; the BLL enforces).</summary>
    Function CanAccess(session As UserSession, moduleName As String, action As String) As Boolean

    ''' <summary>Writes the logout to the audit log and clears the active company.</summary>
    Sub Logout(userId As Integer)
End Interface

