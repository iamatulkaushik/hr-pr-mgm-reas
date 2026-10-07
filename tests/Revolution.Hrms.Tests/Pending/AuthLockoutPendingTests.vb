' PENDING: needs Dev Brave's AuthService (BLL) and IAuthDal (DAL), not merged yet.
' Not compiled until you build with -p:BraveMerged=true. Contract these tests assume:
'   New AuthService(dal As IAuthDal, settings As SecuritySettings)   implements IAuthService
'   SecuritySettings.MaxFailedLogins = 5, LockoutMinutes = 15 (defaults)
'   IAuthDal: GetLoginInfo, RecordSuccess, RecordFailure (returns True when this failure locked the account),
'             RecordUnknownUser, ChangePassword, GetPermissionCodes, GetCompanies
'   LoginInfo (Models): UserId, Username, PasswordHash, Salt, HashIterations, Kind, RoleId, FailedCount,
'             LockedUntilUtc, MustChangePassword, IsActive, ServerNowUtc
' If Brave's names differ, change this file. The rules being tested must not change:
' lock after 5 failed logins, no hint whether the user exists, locked accounts are not checked, inactive needs the right password.
Imports Revolution.Hrms.BLL
Imports Revolution.Hrms.Common
Imports Revolution.Hrms.DAL
Imports Revolution.Hrms.Models
Imports Xunit

Public Class AuthLockoutPendingTests

    Private Const PASSWORD As String = "Right#Pass1"

    Private NotInheritable Class FakeAuthDal
        Implements IAuthDal

        Public ReadOnly Property Users As New Dictionary(Of String, LoginInfo)(StringComparer.OrdinalIgnoreCase)
        Public Property NowUtc As Date = New Date(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc)
        Public Property MaxFailed As Integer
        Public Property SuccessCount As Integer
        Public Property UnknownCount As Integer

        Public Function GetLoginInfo(username As String) As LoginInfo Implements IAuthDal.GetLoginInfo
            Dim info As LoginInfo = Nothing
            If Users.TryGetValue(username, info) Then
                info.ServerNowUtc = NowUtc
                Return info
            End If
            Return Nothing
        End Function

        Public Sub RecordSuccess(userId As Integer, machine As String) Implements IAuthDal.RecordSuccess
            SuccessCount += 1
        End Sub

        Public Function RecordFailure(userId As Integer, maxFailed As Integer, lockMinutes As Integer, machine As String) As Boolean Implements IAuthDal.RecordFailure
            Dim info = Users.Values.Single(Function(u) u.UserId = userId)
            info.FailedCount += 1
            If info.FailedCount >= maxFailed Then
                info.FailedCount = 0
                info.LockedUntilUtc = NowUtc.AddMinutes(lockMinutes)
                Return True
            End If
            Return False
        End Function

        Public Sub RecordUnknownUser(machine As String) Implements IAuthDal.RecordUnknownUser
            UnknownCount += 1
        End Sub

        Public Sub ChangePassword(userId As Integer, hash As Byte(), salt As Byte(), iterations As Integer, machine As String) Implements IAuthDal.ChangePassword
        End Sub

        Public Function GetPermissionCodes(userId As Integer) As IReadOnlyList(Of String) Implements IAuthDal.GetPermissionCodes
            Return New List(Of String) From {"Employee.View"}
        End Function

        Public Function GetCompanies(userId As Integer) As IReadOnlyList(Of CompanyInfo) Implements IAuthDal.GetCompanies
            Return New List(Of CompanyInfo) From {New CompanyInfo(1, "Alpha")}
        End Function
    End Class

    Private Shared Function CreateService(dal As FakeAuthDal, Optional isActive As Boolean = True) As AuthService
        Dim hashed = PasswordHasher.CreateHash(PASSWORD)
        dal.Users("asha") = New LoginInfo With {
            .UserId = 7, .Username = "asha", .PasswordHash = hashed.Hash, .Salt = hashed.Salt,
            .HashIterations = hashed.Iterations, .Kind = UserType.[Operator], .RoleId = 3, .IsActive = isActive}
        Return New AuthService(dal, New SecuritySettings())
    End Function

    <Fact>
    Public Sub FifthWrongPassword_LocksTheAccount()
        Dim dal As New FakeAuthDal()
        Dim service = CreateService(dal)

        For attempt = 1 To 4
            Assert.Equal(LoginOutcome.InvalidCredentials, service.Login("asha", "wrong", "PC1").Outcome)
        Next
        Assert.Equal(LoginOutcome.AccountLocked, service.Login("asha", "wrong", "PC1").Outcome)
    End Sub

    <Fact>
    Public Sub LockedAccount_RefusesTheCorrectPassword()
        Dim dal As New FakeAuthDal()
        Dim service = CreateService(dal)
        For attempt = 1 To 5
            service.Login("asha", "wrong", "PC1")
        Next

        Dim result = service.Login("asha", PASSWORD, "PC1")

        Assert.Equal(LoginOutcome.AccountLocked, result.Outcome)
        Assert.Equal(0, dal.SuccessCount)
    End Sub

    <Fact>
    Public Sub AfterTheLockExpires_TheCorrectPasswordWorks()
        Dim dal As New FakeAuthDal()
        Dim service = CreateService(dal)
        For attempt = 1 To 5
            service.Login("asha", "wrong", "PC1")
        Next

        dal.NowUtc = dal.NowUtc.AddMinutes(16)
        Dim result = service.Login("asha", PASSWORD, "PC1")

        Assert.Equal(LoginOutcome.Success, result.Outcome)
    End Sub

    <Fact>
    Public Sub FourWrongThenRight_Succeeds()
        Dim dal As New FakeAuthDal()
        Dim service = CreateService(dal)
        For attempt = 1 To 4
            service.Login("asha", "wrong", "PC1")
        Next

        Dim result = service.Login("asha", PASSWORD, "PC1")

        Assert.Equal(LoginOutcome.Success, result.Outcome)
        Assert.Equal(1, dal.SuccessCount)
        Assert.True(result.Session.CanAccess("Employee", "View"))
        Assert.Single(result.Session.Companies)
    End Sub

    <Fact>
    Public Sub UnknownUser_LooksLikeAWrongPassword_AndIsRecorded()
        Dim dal As New FakeAuthDal()
        Dim service = CreateService(dal)

        Dim result = service.Login("nobody", "whatever", "PC1")

        Assert.Equal(LoginOutcome.InvalidCredentials, result.Outcome)
        Assert.Equal(1, dal.UnknownCount)
    End Sub

    <Fact>
    Public Sub InactiveUser_WithTheRightPassword_IsToldItIsInactive()
        Dim dal As New FakeAuthDal()
        Dim service = CreateService(dal, isActive:=False)

        Assert.Equal(LoginOutcome.AccountInactive, service.Login("asha", PASSWORD, "PC1").Outcome)
    End Sub

    <Fact>
    Public Sub InactiveUser_WithAWrongPassword_LooksLikeAWrongPassword()
        Dim dal As New FakeAuthDal()
        Dim service = CreateService(dal, isActive:=False)

        Assert.Equal(LoginOutcome.InvalidCredentials, service.Login("asha", "wrong", "PC1").Outcome)
    End Sub

End Class
