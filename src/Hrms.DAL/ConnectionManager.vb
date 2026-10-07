Option Strict On
Imports System.Data
Imports Microsoft.Data.SqlClient
Imports Hrms.Common

Public Enum DbTarget
    Master = 1
    Data = 2
End Enum

''' <summary>Who is calling. Data DB calls need CompanyID; it becomes SESSION_CONTEXT for row-level security.</summary>
Public Class DbContext
    Public Property CompanyID As Integer?
    Public Property UserID As Integer?
    Public Sub New(Optional companyId As Integer? = Nothing, Optional userId As Integer? = Nothing)
        Me.CompanyID = companyId
        Me.UserID = userId
    End Sub
End Class

Public NotInheritable Class ConnectionManager
    Private Shared _cs As ConnectionStrings
    Private Shared ReadOnly Sync As New Object()

    Private Shared Function Strings() As ConnectionStrings
        SyncLock Sync
            If _cs Is Nothing Then _cs = ConnectionStringStore.Load()
            Return _cs
        End SyncLock
    End Function

    ''' <summary>Opens a pooled connection. Caller disposes. Data connections get the company context first.</summary>
    Public Shared Function Open(target As DbTarget, ctx As DbContext) As SqlConnection
        Dim cs = Strings()
        Dim conn As New SqlConnection(If(target = DbTarget.Master, cs.Master, cs.Data))
        Try
            conn.Open()
            If target = DbTarget.Data Then ApplyContext(conn, ctx)
            Return conn
        Catch
            conn.Dispose()
            Throw
        End Try
    End Function

    Private Shared Sub ApplyContext(conn As SqlConnection, ctx As DbContext)
        If ctx Is Nothing OrElse Not ctx.CompanyID.HasValue Then
            Throw New InvalidOperationException("Company not selected.")
        End If
        Using cmd = conn.CreateCommand()
            cmd.CommandText = "sec.usp_Session_SetCompany"
            cmd.CommandType = CommandType.StoredProcedure
            cmd.Parameters.AddWithValue("@CompanyID", ctx.CompanyID.Value)
            cmd.Parameters.AddWithValue("@UserID", If(ctx.UserID.HasValue, CObj(ctx.UserID.Value), DBNull.Value))
            cmd.ExecuteNonQuery()
        End Using
    End Sub

    ''' <summary>Setup tool / tests: forget cached strings.</summary>
    Public Shared Sub Reset()
        SyncLock Sync
            _cs = Nothing
        End SyncLock
    End Sub
End Class
