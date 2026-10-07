#If STUB_AUTH Then
Imports Hrms.BLL
Imports Hrms.Models

''' <summary>
''' DEV ONLY. A fake sign-in service with hard-coded users so the screens can be tried before the real BLL exists.
''' It is compiled only with -p:UseStubAuth=true and a Debug build (a Release build with it fails on purpose).
''' Never ship it. Users: associate, company, operator, newuser (must change password), nocompany. Password: Stub#1234.
''' It mimics the 5-failed-logins lock. Permission lists mirror Database\Seed\Reference\001_roles_permissions.sql.
''' </summary>
Friend NotInheritable Class StubAuthService
    Implements IAuthService

    Private Const STUB_PASSWORD As String = "Stub#1234"
    Private Const MAX_FAILED As Integer = 5
    Private Const LOCK_MINUTES As Integer = 15
    Private Const MIN_LENGTH As Integer = 8

    Private NotInheritable Class StubUser
        Public Property Id As Integer
        Public Property Username As String = String.Empty
        Public Property Password As String = STUB_PASSWORD
        Public Property Kind As UserType
        Public Property MustChange As Boolean
        Public Property Failed As Integer
        Public Property LockedUntilUtc As Date?
        Public ReadOnly Property Companies As New List(Of CompanyInfo)()
    End Class

    Private Shared ReadOnly _associate As String() = {
        "Company.View", "Company.Edit", "Employee.View", "Employee.Edit", "Attendance.View", "Attendance.Edit",
        "Attendance.Unlock", "Entry.View", "Entry.Edit", "Payroll.View", "Payroll.Process", "Payroll.Approve",
        "Payslip.View", "Statutory.View", "Statutory.Edit", "Rates.Edit", "Career.View", "Career.Edit",
        "Reports.View", "Reports.Payroll", "Users.View", "Users.Edit", "Backup.Run", "Audit.View"}

    Private Shared ReadOnly _company As String() = {
        "Company.View", "Employee.View", "Employee.Edit", "Attendance.View", "Attendance.Edit", "Attendance.Unlock",
        "Entry.View", "Entry.Edit", "Payroll.View", "Payroll.Process", "Payroll.Approve", "Payslip.View",
        "Statutory.View", "Statutory.Edit", "Career.View", "Career.Edit", "Reports.View", "Reports.Payroll",
        "Users.View", "Users.Edit", "Audit.View"}

    Private Shared ReadOnly _operator As String() = {
        "Employee.View", "Employee.Edit", "Attendance.View", "Attendance.Edit", "Entry.View", "Entry.Edit",
        "Payslip.View", "Reports.View"}

    Private ReadOnly _users As New Dictionary(Of String, StubUser)(StringComparer.OrdinalIgnoreCase)

    Public Sub New()
        Dim alpha As New CompanyInfo(1, "Alpha Industries")
        Dim beta As New CompanyInfo(2, "Beta Textiles")
        Dim gamma As New CompanyInfo(3, "Gamma Foods")

        AddUser(1, "associate", UserType.Associate, False, alpha, beta, gamma)
        AddUser(2, "company", UserType.Company, False, alpha)
        AddUser(3, "operator", UserType.[Operator], False, alpha, beta)
        AddUser(4, "newuser", UserType.[Operator], True, alpha)
        AddUser(5, "nocompany", UserType.[Operator], False)
    End Sub

    Public Function Login(username As String, password As String, machine As String) As LoginResult Implements IAuthService.Login
        Dim user As StubUser = Nothing
        If Not _users.TryGetValue(If(username, String.Empty).Trim(), user) Then
            Return LoginResult.Failed(LoginOutcome.InvalidCredentials)
        End If

        If user.LockedUntilUtc.HasValue AndAlso user.LockedUntilUtc.Value > Date.UtcNow Then
            Return LoginResult.Failed(LoginOutcome.AccountLocked, user.LockedUntilUtc)
        End If

        If password <> user.Password Then
            user.Failed += 1
            If user.Failed >= MAX_FAILED Then
                user.Failed = 0
                user.LockedUntilUtc = Date.UtcNow.AddMinutes(LOCK_MINUTES)
                Return LoginResult.Failed(LoginOutcome.AccountLocked, user.LockedUntilUtc)
            End If
            Return LoginResult.Failed(LoginOutcome.InvalidCredentials)
        End If

        user.Failed = 0
        Dim session As New UserSession(user.Id, user.Username, user.Kind, PermissionsFor(user.Kind), user.Companies, user.MustChange)
        Return LoginResult.Succeeded(session)
    End Function

    Public Function ChangePassword(session As UserSession, currentPassword As String, newPassword As String, machine As String) As PasswordChangeResult Implements IAuthService.ChangePassword
        Dim user = _users(session.Username)
        If currentPassword <> user.Password Then Return PasswordChangeResult.WrongCurrent()
        If newPassword.Length < MIN_LENGTH Then
            Return PasswordChangeResult.Rejected($"The password must have at least {MIN_LENGTH} characters.")
        End If
        If newPassword = currentPassword Then
            Return PasswordChangeResult.Rejected("The new password must be different from the current one.")
        End If

        user.Password = newPassword
        user.MustChange = False
        session.MustChangePassword = False
        Return PasswordChangeResult.Changed()
    End Function

    Public Function VerifyPassword(session As UserSession, password As String) As Boolean Implements IAuthService.VerifyPassword
        Return _users(session.Username).Password = password
    End Function

    Private Sub AddUser(id As Integer, username As String, kind As UserType, mustChange As Boolean, ParamArray companies As CompanyInfo())
        Dim user As New StubUser() With {.Id = id, .Username = username, .Kind = kind, .MustChange = mustChange}
        user.Companies.AddRange(companies)
        _users(username) = user
    End Sub

    Private Shared Function PermissionsFor(kind As UserType) As String()
        Select Case kind
            Case UserType.Associate
                Return _associate
            Case UserType.Company
                Return _company
            Case Else
                Return _operator
        End Select
    End Function

End Class
#End If
