Option Strict On
Imports System.Data
Imports Hrms.Models
Imports Hrms.DAL.ProcHelper

Public Class UserRecord
    Public Property UserID As Integer
    Public Property Username As String = ""
    Public Property FullName As String = ""
    Public Property PasswordHash As Byte()
    Public Property Salt As Byte()
    Public Property Iterations As Integer
    Public Property RoleID As Integer
    Public Property UserType As String = ""
    Public Property FailedCount As Integer
    Public Property LockedUntilUtc As DateTime?
    Public Property MustChangePassword As Boolean
    Public Property IsActive As Boolean
End Class

Public Class AuthRepository
    Private ReadOnly _db As New ProcHelper(DbTarget.Master)

    Private Shared Function NullableDate(v As Object) As DateTime?
        If IsDBNull(v) Then Return Nothing
        Return CDate(v)
    End Function

    Public Function GetUser(username As String) As UserRecord
        Dim dt = _db.Table("sec.usp_User_GetByUsername", P("@Username", username))
        If dt.Rows.Count = 0 Then Return Nothing
        Dim r = dt.Rows(0)
        Return New UserRecord With {
            .UserID = CInt(r("UserID")),
            .Username = CStr(r("Username")),
            .FullName = CStr(r("FullName")),
            .PasswordHash = CType(r("PasswordHash"), Byte()),
            .Salt = CType(r("Salt"), Byte()),
            .Iterations = CInt(r("Iterations")),
            .RoleID = CInt(r("RoleID")),
            .UserType = CStr(r("UserType")),
            .FailedCount = CInt(r("FailedCount")),
            .LockedUntilUtc = NullableDate(r("LockedUntil")),
            .MustChangePassword = CBool(r("MustChangePwd")),
            .IsActive = CBool(r("IsActive"))
        }
    End Function

    Public Function RecordFailure(userId As Integer, maxFailed As Integer, lockMinutes As Integer) As (FailedCount As Integer, LockedUntilUtc As DateTime?)
        Dim dt = _db.Table("sec.usp_User_LoginFailed", P("@UserID", userId), P("@MaxFailed", maxFailed), P("@LockMinutes", lockMinutes))
        Dim r = dt.Rows(0)
        Return (CInt(r("FailedCount")), NullableDate(r("LockedUntil")))
    End Function

    Public Sub RecordSuccess(userId As Integer)
        _db.NonQuery("sec.usp_User_LoginSucceeded", P("@UserID", userId))
    End Sub

    Public Function CreateUser(username As String, fullName As String, email As String, mobile As String,
                               hash As Byte(), salt As Byte(), iterations As Integer, roleId As Integer, createdBy As Integer?) As Integer
        Dim outId = OutInt("@NewUserID")
        _db.NonQuery("sec.usp_User_Create", P("@Username", username), P("@FullName", fullName), P("@Email", email), P("@Mobile", mobile),
                     P("@PasswordHash", hash), P("@Salt", salt), P("@Iterations", iterations), P("@RoleID", roleId),
                     P("@CreatedBy", createdBy), outId)
        Return CInt(outId.Value)
    End Function

    Public Sub SetPassword(userId As Integer, hash As Byte(), salt As Byte(), iterations As Integer, mustChange As Boolean)
        _db.NonQuery("sec.usp_User_SetPassword", P("@UserID", userId), P("@PasswordHash", hash), P("@Salt", salt),
                     P("@Iterations", iterations), P("@MustChange", mustChange))
    End Sub

    Public Function CountUsers() As Integer
        Return _db.Scalar(Of Integer)("sec.usp_User_Count")
    End Function

    Public Function GetRoleId(userType As String) As Integer
        Return _db.Scalar(Of Integer)("sec.usp_Role_GetByType", P("@UserType", userType))
    End Function

    Public Function GetPermissions(userId As Integer) As List(Of String)
        Dim list As New List(Of String)
        For Each row As DataRow In _db.Table("sec.usp_Permission_ListByUser", P("@UserID", userId)).Rows
            list.Add(CStr(row("Code")))
        Next
        Return list
    End Function

    Public Function GetCompanies(userId As Integer) As List(Of CompanyInfo)
        Dim list As New List(Of CompanyInfo)
        For Each row As DataRow In _db.Table("sec.usp_UserCompany_ListByUser", P("@UserID", userId)).Rows
            list.Add(New CompanyInfo With {.CompanyID = CInt(row("CompanyID")), .CompanyCode = CStr(row("CompanyCode")), .CompanyName = CStr(row("CompanyName"))})
        Next
        Return list
    End Function

    Public Function IsCompanyAllowed(userId As Integer, companyId As Integer) As Boolean
        Return _db.Scalar(Of Boolean)("sec.usp_UserCompany_Check", P("@UserID", userId), P("@CompanyID", companyId))
    End Function

    Public Function GetSettings() As Dictionary(Of String, String)
        Dim d As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each row As DataRow In _db.Table("cfg.usp_Setting_GetAll").Rows
            d(CStr(row("SettingKey"))) = CStr(row("SettingValue"))
        Next
        Return d
    End Function
End Class

Public Class AuditRepository
    Private ReadOnly _db As New ProcHelper(DbTarget.Master)

    Public Sub Write(userId As Integer?, username As String, companyId As Integer?, machine As String,
                     action As String, tableName As String, recordKey As String, detail As String)
        _db.NonQuery("aud.usp_AuditLog_Write", P("@UserID", userId), P("@Username", username), P("@CompanyID", companyId),
                     P("@MachineName", machine), P("@Action", action), P("@TableName", tableName),
                     P("@RecordKey", recordKey), P("@Detail", detail))
    End Sub
End Class
