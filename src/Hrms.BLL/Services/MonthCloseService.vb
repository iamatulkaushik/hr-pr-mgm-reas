Imports Hrms.Models

Public NotInheritable Class MonthCloseService

    Public Const MIN_REASON_LENGTH As Integer = 10
    Private Const MIN_YEAR As Integer = 2000
    Private Const MAX_YEAR As Integer = 2100

    Private ReadOnly _repository As IMonthCloseRepository
    Private ReadOnly _user As IUserContext

    Public Sub New(repository As IMonthCloseRepository, user As IUserContext)
        _repository = repository
        _user = user
    End Sub

    Public Shared Function MonthHasEnded(year As Integer, month As Integer) As Boolean
        Dim lastDay As New Date(year, month, Date.DaysInMonth(year, month))
        Return lastDay < Date.Today
    End Function

    Public Function GetStatus(year As Integer, month As Integer) As MonthCloseStatus
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.VIEW)
        ValidateMonth(year, month)
        Return _repository.GetStatus(_user.CompanyId, year, month)
    End Function

    Public Function CountMissingMonthly(year As Integer, month As Integer) As Integer
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.VIEW)
        ValidateMonth(year, month)
        Return _repository.CountMissingMonthly(_user.CompanyId, year, month)
    End Function

    Public Function GetLog(year As Integer, month As Integer) As List(Of MonthCloseLogEntry)
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.VIEW)
        ValidateMonth(year, month)
        Return _repository.GetLog(_user.CompanyId, year, month)
    End Function

    Public Sub CloseMonth(year As Integer, month As Integer)
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.MONTH_CLOSE)
        ValidateMonth(year, month)
        If New Date(year, month, 1) > Date.Today Then
            Throw New BusinessRuleException("A future month cannot be closed.")
        End If
        If _repository.GetStatus(_user.CompanyId, year, month).IsClosed Then
            Throw New BusinessRuleException($"Attendance for {month:00}-{year} is already closed.")
        End If
        _repository.CloseMonth(_user.CompanyId, year, month, _user.UserId)
    End Sub

    Public Sub ReopenMonth(year As Integer, month As Integer, reason As String)
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.REOPEN)
        ValidateMonth(year, month)
        Dim cleaned As String = If(reason, String.Empty).Trim()
        If cleaned.Length < MIN_REASON_LENGTH Then
            Throw New BusinessRuleException($"Enter a reason of at least {MIN_REASON_LENGTH} characters.")
        End If
        If Not _repository.GetStatus(_user.CompanyId, year, month).IsClosed Then
            Throw New BusinessRuleException($"Attendance for {month:00}-{year} is not closed.")
        End If
        _repository.ReopenMonth(_user.CompanyId, year, month, _user.UserId, cleaned)
    End Sub

    Private Shared Sub ValidateMonth(year As Integer, month As Integer)
        If year < MIN_YEAR OrElse year > MAX_YEAR OrElse month < 1 OrElse month > 12 Then
            Throw New BusinessRuleException("Choose a valid month.")
        End If
    End Sub

End Class
