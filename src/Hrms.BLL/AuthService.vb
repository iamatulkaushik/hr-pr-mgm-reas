Option Strict On
Imports Hrms.Common
Imports Hrms.DAL
Imports Hrms.Models

Public Class AuthService
    Implements IAuthService

    Private Const GenericFail As String = "Invalid username or password."
    Private ReadOnly _repo As New AuthRepository()
    Private ReadOnly _audit As New AuditService()

    Private Shared Function Fail(message As String, Optional lockedUntil As DateTime? = Nothing) As AuthResult
        Return New AuthResult With {.Success = False, .Message = message, .LockedUntilUtc = lockedUntil}
    End Function

    Private Function Setting(key As String, fallback As Integer, settings As Dictionary(Of String, String)) As Integer
        Dim v As String = Nothing
        Dim n As Integer
        If settings.TryGetValue(key, v) AndAlso Integer.TryParse(v, n) AndAlso n > 0 Then Return n
        Return fallback
    End Function

    Public Function Login(username As String, password As String) As AuthResult Implements IAuthService.Login
        username = If(username, "").Trim()
        If username.Length = 0 OrElse String.IsNullOrEmpty(password) Then Return Fail(GenericFail)

        Dim settings = _repo.GetSettings()
        Dim maxFailed = Setting("MaxFailedLogins", 5, settings)
        Dim lockMinutes = Setting("LockoutMinutes", 15, settings)

        Dim u = _repo.GetUser(username)
        If u Is Nothing Then
            ' burn the same time as a real check so usernames cannot be probed
            CryptoHelper.HashPassword(password, CryptoHelper.NewSalt(), CryptoHelper.DefaultIterations)
            _audit.LogAnonymous(username, Nothing, "LoginFailed", "Unknown user")
            Return Fail(GenericFail)
        End If

        If Not u.IsActive Then
            _audit.LogAnonymous(username, u.UserID, "LoginFailed", "Inactive user")
            Return Fail(GenericFail)
        End If

        If u.LockedUntilUtc.HasValue AndAlso u.LockedUntilUtc.Value > DateTime.UtcNow Then
            Dim mins = CInt(Math.Ceiling((u.LockedUntilUtc.Value - DateTime.UtcNow).TotalMinutes))
            _audit.LogAnonymous(username, u.UserID, "LoginBlocked", "Account locked")
            Return Fail($"Account locked. Try again in {mins} minute(s).", u.LockedUntilUtc)
        End If

        If Not CryptoHelper.VerifyPassword(password, u.Salt, u.Iterations, u.PasswordHash) Then
            Dim f = _repo.RecordFailure(u.UserID, maxFailed, lockMinutes)
            _audit.LogAnonymous(username, u.UserID, "LoginFailed", $"Bad password, count {f.FailedCount}")
            If f.LockedUntilUtc.HasValue AndAlso f.LockedUntilUtc.Value > DateTime.UtcNow Then
                _audit.LogAnonymous(username, u.UserID, "AccountLocked", $"Locked for {lockMinutes} minutes")
                Return Fail($"Too many failed attempts. Account locked for {lockMinutes} minutes.", f.LockedUntilUtc)
            End If
            Return Fail(GenericFail)
        End If

        _repo.RecordSuccess(u.UserID)

        Dim s As New SessionInfo With {
            .UserID = u.UserID,
            .Username = u.Username,
            .FullName = u.FullName,
            .RoleID = u.RoleID,
            .UserType = CType([Enum].Parse(GetType(UserType), u.UserType), UserType),
            .MustChangePassword = u.MustChangePassword
        }
        For Each code In _repo.GetPermissions(u.UserID)
            s.Permissions.Add(code)
        Next
        s.AllowedCompanies.AddRange(_repo.GetCompanies(u.UserID))

        _audit.Log(s, "Login", "sec.User", u.UserID.ToString())
        Return New AuthResult With {.Success = True, .Session = s, .Message = If(u.MustChangePassword, "Change your password to continue.", "")}
    End Function

    Public Sub Logout(session As SessionInfo) Implements IAuthService.Logout
        If session IsNot Nothing Then _audit.Log(session, "Logout", "sec.User", session.UserID.ToString())
    End Sub

    Public Function ChangePassword(session As SessionInfo, oldPassword As String, newPassword As String) As OperationResult Implements IAuthService.ChangePassword
        If session Is Nothing Then Return OperationResult.Fail("Not signed in.")
        Dim u = _repo.GetUser(session.Username)
        If u Is Nothing OrElse Not u.IsActive Then Return OperationResult.Fail("Not signed in.")
        If Not CryptoHelper.VerifyPassword(If(oldPassword, ""), u.Salt, u.Iterations, u.PasswordHash) Then
            _audit.Log(session, "PasswordChangeFailed", "sec.User", u.UserID.ToString(), "Wrong current password")
            Return OperationResult.Fail("Current password is wrong.")
        End If
        If oldPassword = newPassword Then Return OperationResult.Fail("New password must differ from the current one.")
        Dim problem = PasswordPolicy.Validate(newPassword, session.Username)
        If problem IsNot Nothing Then Return OperationResult.Fail(problem)

        Dim salt = CryptoHelper.NewSalt()
        Dim hash = CryptoHelper.HashPassword(newPassword, salt, CryptoHelper.DefaultIterations)
        _repo.SetPassword(u.UserID, hash, salt, CryptoHelper.DefaultIterations, False)
        session.MustChangePassword = False
        _audit.Log(session, "PasswordChanged", "sec.User", u.UserID.ToString())
        Return OperationResult.Ok("Password changed.")
    End Function

    Public Function SelectCompany(session As SessionInfo, companyId As Integer) As OperationResult Implements IAuthService.SelectCompany
        If session Is Nothing Then Return OperationResult.Fail("Not signed in.")
        If session.MustChangePassword Then Return OperationResult.Fail("Change your password first.")
        Dim company = session.AllowedCompanies.FirstOrDefault(Function(c) c.CompanyID = companyId)
        If company Is Nothing OrElse Not _repo.IsCompanyAllowed(session.UserID, companyId) Then
            _audit.Log(session, "CompanySelectDenied", "mst.Company", companyId.ToString())
            Return OperationResult.Fail("You do not have access to this company.")
        End If
        session.CurrentCompany = company
        _audit.Log(session, "CompanySelect", "mst.Company", companyId.ToString())
        Return OperationResult.Ok()
    End Function

    Public Function CanAccess(session As SessionInfo, moduleName As String, action As String) As Boolean Implements IAuthService.CanAccess
        If session Is Nothing OrElse session.MustChangePassword Then Return False
        Return session.Permissions.Contains(moduleName & "." & action)
    End Function

    Public Sub Demand(session As SessionInfo, permissionCode As String) Implements IAuthService.Demand
        If session Is Nothing OrElse session.MustChangePassword OrElse Not session.Permissions.Contains(permissionCode) Then
            _audit.Log(session, "PermissionDenied", Nothing, Nothing, permissionCode)
            Throw New PermissionDeniedException(permissionCode)
        End If
        ' company removal takes effect on the user's next action
        If session.CurrentCompany IsNot Nothing AndAlso Not _repo.IsCompanyAllowed(session.UserID, session.CurrentCompany.CompanyID) Then
            session.CurrentCompany = Nothing
            _audit.Log(session, "PermissionDenied", Nothing, Nothing, "Company access removed")
            Throw New PermissionDeniedException("company access")
        End If
    End Sub

    Public Function NeedsBootstrap() As Boolean Implements IAuthService.NeedsBootstrap
        Return _repo.CountUsers() = 0
    End Function

    ''' <summary>First-run only: creates the first Associate. Must change password at first login.</summary>
    Public Function BootstrapFirstAssociate(username As String, fullName As String, password As String) As OperationResult Implements IAuthService.BootstrapFirstAssociate
        If _repo.CountUsers() > 0 Then Return OperationResult.Fail("Setup already done.")
        username = If(username, "").Trim()
        If username.Length < 3 Then Return OperationResult.Fail("Username must be at least 3 characters.")
        If String.IsNullOrWhiteSpace(fullName) Then Return OperationResult.Fail("Full name is required.")
        Dim problem = PasswordPolicy.Validate(password, username)
        If problem IsNot Nothing Then Return OperationResult.Fail(problem)

        Dim salt = CryptoHelper.NewSalt()
        Dim hash = CryptoHelper.HashPassword(password, salt, CryptoHelper.DefaultIterations)
        Dim id = _repo.CreateUser(username, fullName.Trim(), Nothing, Nothing, hash, salt, CryptoHelper.DefaultIterations,
                                  _repo.GetRoleId("Associate"), Nothing)
        _audit.LogAnonymous(username, id, "UserCreated", "First Associate (bootstrap)")
        Return OperationResult.Ok("First Associate created.")
    End Function
End Class
