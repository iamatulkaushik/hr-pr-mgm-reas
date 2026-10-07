Imports Hrms.Models

Public NotInheritable Class AttendanceService

    Private Const DEFAULT_WEEKLY_OFF As DayOfWeek = DayOfWeek.Sunday
    Private Const MAX_ERRORS_SHOWN As Integer = 5
    Private Const MIN_YEAR As Integer = 2000
    Private Const MAX_YEAR As Integer = 2100

    Private ReadOnly _attendance As IAttendanceRepository
    Private ReadOnly _monthClose As IMonthCloseRepository
    Private ReadOnly _holidays As IHolidayRepository
    Private ReadOnly _user As IUserContext

    Public Sub New(attendance As IAttendanceRepository, monthClose As IMonthCloseRepository,
                   holidays As IHolidayRepository, user As IUserContext)
        _attendance = attendance
        _monthClose = monthClose
        _holidays = holidays
        _user = user
    End Sub

    Public Function IsMonthClosed(year As Integer, month As Integer) As Boolean
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.VIEW)
        Return _monthClose.GetStatus(_user.CompanyId, year, month).IsClosed
    End Function

    Public Function GetDay(day As Date) As List(Of DailyAttendanceRow)
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.VIEW)
        Return _attendance.GetDaily(_user.CompanyId, day.Date)
    End Function

    Public Function SuggestStatus(day As Date) As String
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.VIEW)
        Dim found As List(Of HolidayDto) = _holidays.List(_user.CompanyId, day.Date, day.Date)
        Dim dates As New HashSet(Of Date)(found.Select(Function(h) h.HolidayDate.Date))
        Return AttendanceEngine.SuggestStatus(day, dates, DEFAULT_WEEKLY_OFF)
    End Function

    Public Function SaveDay(day As Date, rows As IEnumerable(Of DailyAttendanceRow)) As Integer
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.EDIT)
        If day.Date > Date.Today Then
            Throw New BusinessRuleException("Attendance cannot be entered for a future date.")
        End If
        EnsureMonthOpen(day.Year, day.Month)
        Dim toSave As List(Of DailyAttendanceRow) = rows.Where(Function(r) Not r.IsLocked).ToList()
        Dim errors As New List(Of String)()
        For Each r As DailyAttendanceRow In toSave
            r.Status = AttendanceCodes.Normalize(r.Status)
            r.AttDate = day.Date
            For Each message As String In AttendanceEngine.ValidateDay(r)
                errors.Add($"{r.EmpCode}: {message}")
            Next
        Next
        ThrowIfAny(errors)
        If toSave.Count = 0 Then
            Return 0
        End If
        Return _attendance.SaveDaily(_user.CompanyId, day.Date, _user.UserId, toSave)
    End Function

    Public Function GetMonthly(year As Integer, month As Integer) As List(Of MonthlyAttendanceRow)
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.VIEW)
        ValidateMonth(year, month)
        Return _attendance.GetMonthly(_user.CompanyId, year, month)
    End Function

    ' Replaces the values of employees that have daily rows; employees without daily rows keep their monthly values.
    Public Function FillMonthlyFromDaily(year As Integer, month As Integer) As List(Of MonthlyAttendanceRow)
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.VIEW)
        ValidateMonth(year, month)
        Dim monthly As List(Of MonthlyAttendanceRow) = _attendance.GetMonthly(_user.CompanyId, year, month)
        Dim daily As Dictionary(Of Integer, List(Of DailyAttendanceRow)) =
            _attendance.GetDailyForMonth(_user.CompanyId, year, month) _
                .GroupBy(Function(d) d.EmployeeId) _
                .ToDictionary(Function(g) g.Key, Function(g) g.ToList())
        For Each row As MonthlyAttendanceRow In monthly
            Dim entries As List(Of DailyAttendanceRow) = Nothing
            If Not row.IsLocked AndAlso daily.TryGetValue(row.EmployeeId, entries) Then
                CopyValues(AttendanceEngine.Summarize(entries, year, month), row)
            End If
        Next
        Return monthly
    End Function

    Public Function SaveMonthly(year As Integer, month As Integer, rows As IEnumerable(Of MonthlyAttendanceRow)) As Integer
        Guard.Require(_user, AccessModules.ATTENDANCE, AccessActions.EDIT)
        ValidateMonth(year, month)
        EnsureMonthOpen(year, month)
        Dim days As Integer = AttendanceEngine.DaysInMonth(year, month)
        Dim toSave As List(Of MonthlyAttendanceRow) = rows.Where(Function(r) Not r.IsLocked AndAlso (r.HasRecord OrElse HasAnyValue(r))).ToList()
        Dim errors As New List(Of String)()
        For Each r As MonthlyAttendanceRow In toSave
            For Each message As String In AttendanceEngine.ValidateMonthly(r, days)
                errors.Add($"{r.EmpCode}: {message}")
            Next
        Next
        ThrowIfAny(errors)
        If toSave.Count = 0 Then
            Return 0
        End If
        For Each r As MonthlyAttendanceRow In toSave
            r.Year = year
            r.Month = month
            AttendanceEngine.ApplyDerived(r, days)
        Next
        Return _attendance.SaveMonthly(_user.CompanyId, year, month, _user.UserId, toSave)
    End Function

    Private Sub EnsureMonthOpen(year As Integer, month As Integer)
        If _monthClose.GetStatus(_user.CompanyId, year, month).IsClosed Then
            Throw New BusinessRuleException($"Attendance for {month:00}-{year} is closed. Only a Company admin or Associate can reopen it.")
        End If
    End Sub

    Private Shared Sub ValidateMonth(year As Integer, month As Integer)
        If year < MIN_YEAR OrElse year > MAX_YEAR OrElse month < 1 OrElse month > 12 Then
            Throw New BusinessRuleException("Choose a valid month.")
        End If
    End Sub

    Private Shared Function HasAnyValue(row As MonthlyAttendanceRow) As Boolean
        Return row.DaysWorked <> 0D OrElse row.WeeklyOff <> 0D OrElse row.Holidays <> 0D OrElse row.CasualLeave <> 0D _
            OrElse row.EarnedLeave <> 0D OrElse row.SickLeave <> 0D OrElse row.CompLeave <> 0D OrElse row.OtHours <> 0D
    End Function

    Private Shared Sub CopyValues(source As MonthlyAttendanceRow, target As MonthlyAttendanceRow)
        target.DaysWorked = source.DaysWorked
        target.WeeklyOff = source.WeeklyOff
        target.Holidays = source.Holidays
        target.CasualLeave = source.CasualLeave
        target.EarnedLeave = source.EarnedLeave
        target.SickLeave = source.SickLeave
        target.CompLeave = source.CompLeave
        target.Absent = source.Absent
        target.OtHours = source.OtHours
        target.DaysPaid = source.DaysPaid
    End Sub

    Private Shared Sub ThrowIfAny(errors As List(Of String))
        If errors.Count = 0 Then
            Return
        End If
        Dim shown As String = String.Join(Environment.NewLine, errors.Take(MAX_ERRORS_SHOWN))
        If errors.Count > MAX_ERRORS_SHOWN Then
            shown &= Environment.NewLine & $"...and {errors.Count - MAX_ERRORS_SHOWN} more."
        End If
        Throw New BusinessRuleException(shown)
    End Sub

End Class
