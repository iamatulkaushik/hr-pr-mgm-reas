Option Strict On
Imports Hrms.Common
Imports Hrms.DAL
Imports Hrms.Models

''' <summary>Writes to aud.AuditLog. Never pass passwords, hashes or sensitive field values in detail.</summary>
Public Class AuditService
    Private ReadOnly _repo As New AuditRepository()

    Public Sub Log(session As SessionInfo, action As String, Optional tableName As String = Nothing,
                   Optional recordKey As String = Nothing, Optional detail As String = Nothing)
        If session Is Nothing Then
            Write(Nothing, Nothing, Nothing, Environment.MachineName, action, tableName, recordKey, detail)
        Else
            Write(session.UserID, session.Username, session.CurrentCompany?.CompanyID, session.MachineName, action, tableName, recordKey, detail)
        End If
    End Sub

    Public Sub LogAnonymous(username As String, userId As Integer?, action As String, Optional detail As String = Nothing)
        Write(userId, username, Nothing, Environment.MachineName, action, "sec.User", Nothing, detail)
    End Sub

    Private Sub Write(userId As Integer?, username As String, companyId As Integer?, machine As String,
                      action As String, tableName As String, recordKey As String, detail As String)
        Try
            _repo.Write(userId, username, companyId, machine, action, tableName, recordKey, detail)
        Catch ex As Exception
            AppLog.Error("Audit write failed: " & action, ex)
        End Try
    End Sub
End Class
