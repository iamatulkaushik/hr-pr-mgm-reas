Option Strict On
Imports System.Data
Imports Microsoft.Data.SqlClient
Imports Hrms.Common

''' <summary>Runs stored procedures only. No inline SQL anywhere in the app.</summary>
Public Class ProcHelper
    Private ReadOnly _target As DbTarget
    Private ReadOnly _ctx As DbContext

    Public Sub New(target As DbTarget, Optional ctx As DbContext = Nothing)
        _target = target
        _ctx = ctx
    End Sub

    Public Shared Function P(name As String, value As Object) As SqlParameter
        Return New SqlParameter(name, If(value, DBNull.Value))
    End Function

    Public Shared Function OutInt(name As String) As SqlParameter
        Return New SqlParameter(name, SqlDbType.Int) With {.Direction = ParameterDirection.Output}
    End Function

    Public Shared Function OutStr(name As String, size As Integer) As SqlParameter
        Return New SqlParameter(name, SqlDbType.NVarChar, size) With {.Direction = ParameterDirection.Output}
    End Function

    Public Function Table(proc As String, ParamArray args() As SqlParameter) As DataTable
        Return Run(Of DataTable)(proc, args,
            Function(cmd)
                Dim dt As New DataTable()
                Using rdr = cmd.ExecuteReader()
                    dt.Load(rdr)
                End Using
                Return dt
            End Function)
    End Function

    Public Function Scalar(Of T)(proc As String, ParamArray args() As SqlParameter) As T
        Return Run(Of T)(proc, args,
            Function(cmd)
                Dim v = cmd.ExecuteScalar()
                If v Is Nothing OrElse v Is DBNull.Value Then Return Nothing
                Return CType(v, T)
            End Function)
    End Function

    Public Sub NonQuery(proc As String, ParamArray args() As SqlParameter)
        Run(Of Integer)(proc, args, Function(cmd) cmd.ExecuteNonQuery())
    End Sub

    Private Function Run(Of T)(proc As String, args As SqlParameter(), work As Func(Of SqlCommand, T)) As T
        Try
            Using conn = ConnectionManager.Open(_target, _ctx)
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = proc
                    cmd.CommandType = CommandType.StoredProcedure
                    cmd.CommandTimeout = 30
                    If args IsNot Nothing Then cmd.Parameters.AddRange(args)
                    Return work(cmd)
                End Using
            End Using
        Catch ex As SqlException
            ' THROW 50000+ inside a procedure = business rule, safe to show
            If ex.Number >= 50000 Then Throw New BusinessRuleException(ex.Message, ex)
            AppLog.Error($"DB error {ex.Number} in {proc}", ex)
            If ex.Number = 2627 OrElse ex.Number = 2601 Then Throw New BusinessRuleException("Duplicate record.", ex)
            If ex.Number = 547 Then Throw New BusinessRuleException("Record is linked to other data.", ex)
            Throw
        End Try
    End Function
End Class
