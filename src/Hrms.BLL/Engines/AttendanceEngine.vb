Imports Hrms.Models

Public NotInheritable Class AttendanceEngine

    Public Const HALF_STEP As Decimal = 0.5D
    Public Const MAX_DAY_HOURS As Decimal = 24D
    Public Const HOURS_DECIMALS As Integer = 2
    Private Const HOURS_PER_DAY As Integer = 24

    Private Sub New()
    End Sub

    Public Shared Function DaysInMonth(year As Integer, month As Integer) As Integer
        Return Date.DaysInMonth(year, month)
    End Function

    ' Days of presence credited for one daily code (P = 1, HD = 0.5).
    Public Shared Function PresentValue(code As String) As Decimal
        Select Case AttendanceCodes.Normalize(code)
            Case AttendanceCodes.PRESENT
                Return 1D
            Case AttendanceCodes.HALF_DAY
                Return HALF_STEP
            Case Else
                Return 0D
        End Select
    End Function

    ' Days paid for one daily code: weekly off, holidays and paid leave are paid; absent is unpaid.
    Public Shared Function PaidValue(code As String) As Decimal
        Select Case AttendanceCodes.Normalize(code)
            Case AttendanceCodes.PRESENT, AttendanceCodes.WEEKLY_OFF, AttendanceCodes.HOLIDAY,
                 AttendanceCodes.CASUAL_LEAVE, AttendanceCodes.EARNED_LEAVE, AttendanceCodes.SICK_LEAVE, AttendanceCodes.COMP_OFF
                Return 1D
            Case AttendanceCodes.HALF_DAY
                Return HALF_STEP
            Case Else
                Return 0D
        End Select
    End Function

    Public Shared Function IsHalfStep(value As Decimal) As Boolean
        Dim doubled As Decimal = value * 2D
        Return doubled = Math.Truncate(doubled)
    End Function

    Public Shared Function ComputeDaysPaid(row As MonthlyAttendanceRow) As Decimal
        Return row.DaysWorked + row.WeeklyOff + row.Holidays + row.CasualLeave + row.EarnedLeave + row.SickLeave + row.CompLeave
    End Function

    ' Absent is whatever part of the month is not accounted for; DaysPaid follows the payroll rule.
    Public Shared Sub ApplyDerived(row As MonthlyAttendanceRow, daysInMonth As Integer)
        row.DaysPaid = ComputeDaysPaid(row)
        row.Absent = Math.Max(0D, daysInMonth - row.DaysPaid)
    End Sub

    Public Shared Function ValidateMonthly(row As MonthlyAttendanceRow, daysInMonth As Integer) As List(Of String)
        Dim errors As New List(Of String)()
        CheckValue(errors, "Worked", row.DaysWorked, daysInMonth)
        CheckValue(errors, "Weekly off", row.WeeklyOff, daysInMonth)
        CheckValue(errors, "Holidays", row.Holidays, daysInMonth)
        CheckValue(errors, "Casual leave", row.CasualLeave, daysInMonth)
        CheckValue(errors, "Earned leave", row.EarnedLeave, daysInMonth)
        CheckValue(errors, "Sick leave", row.SickLeave, daysInMonth)
        CheckValue(errors, "Comp off", row.CompLeave, daysInMonth)
        If row.OtHours < 0D Then
            errors.Add("Overtime hours cannot be negative.")
        End If
        If errors.Count = 0 AndAlso ComputeDaysPaid(row) > daysInMonth Then
            errors.Add($"Total days ({ComputeDaysPaid(row)}) exceed the {daysInMonth} days in the month.")
        End If
        Return errors
    End Function

    Public Shared Function ValidateDay(row As DailyAttendanceRow) As List(Of String)
        Dim errors As New List(Of String)()
        Dim code As String = AttendanceCodes.Normalize(row.Status)
        If code.Length > 0 AndAlso Not AttendanceCodes.IsValid(code) Then
            errors.Add($"Status '{row.Status}' is not valid.")
        End If
        If row.OtHours < 0D OrElse row.OtHours > MAX_DAY_HOURS Then
            errors.Add($"Overtime must be between 0 and {MAX_DAY_HOURS:0} hours.")
        End If
        If row.OtHours > 0D AndAlso code <> AttendanceCodes.PRESENT AndAlso code <> AttendanceCodes.HALF_DAY Then
            errors.Add("Overtime is only allowed on Present or Half day.")
        End If
        If row.InTime.HasValue Xor row.OutTime.HasValue Then
            errors.Add("Enter both In and Out time.")
        ElseIf row.InTime.HasValue AndAlso row.OutTime.HasValue Then
            If Not IsValidClock(row.InTime.Value) OrElse Not IsValidClock(row.OutTime.Value) Then
                errors.Add("In and Out time must be between 00:00 and 23:59.")
            ElseIf row.InTime.Value = row.OutTime.Value Then
                errors.Add("In and Out time cannot be the same.")
            End If
        End If
        Return errors
    End Function

    ' Overtime counts only above shift hours; an Out time earlier than In time means the shift crosses midnight.
    Public Shared Function OvertimeHours(inTime As TimeSpan, outTime As TimeSpan, settings As OvertimeSettings) As Decimal
        Dim span As TimeSpan = outTime - inTime
        If span <= TimeSpan.Zero Then
            span = span + TimeSpan.FromHours(HOURS_PER_DAY)
        End If
        Dim worked As Decimal = CDec(span.Ticks) / CDec(TimeSpan.TicksPerHour)
        Dim overtime As Decimal = worked - settings.ShiftHours
        If overtime <= 0D Then
            Return 0D
        End If
        Return RoundOvertime(overtime, settings.Rounding, settings.StepHours)
    End Function

    Public Shared Function RoundOvertime(hours As Decimal, mode As OvertimeRounding, stepHours As Decimal) As Decimal
        If mode = OvertimeRounding.None OrElse stepHours <= 0D Then
            Return Math.Round(hours, HOURS_DECIMALS, MidpointRounding.AwayFromZero)
        End If
        Dim units As Decimal = hours / stepHours
        Select Case mode
            Case OvertimeRounding.Down
                units = Math.Floor(units)
            Case OvertimeRounding.Up
                units = Math.Ceiling(units)
            Case Else
                units = Math.Round(units, 0, MidpointRounding.AwayFromZero)
        End Select
        Return units * stepHours
    End Function

    ' Builds the monthly summary of one employee from daily rows. Unmarked days count as absent (unpaid).
    Public Shared Function Summarize(rows As IEnumerable(Of DailyAttendanceRow), year As Integer, month As Integer) As MonthlyAttendanceRow
        Dim result As New MonthlyAttendanceRow() With {.Year = year, .Month = month}
        For Each r As DailyAttendanceRow In rows
            If r.AttDate.Year <> year OrElse r.AttDate.Month <> month Then
                Continue For
            End If
            Select Case AttendanceCodes.Normalize(r.Status)
                Case AttendanceCodes.PRESENT
                    result.DaysWorked += 1D
                Case AttendanceCodes.HALF_DAY
                    result.DaysWorked += HALF_STEP
                Case AttendanceCodes.WEEKLY_OFF
                    result.WeeklyOff += 1D
                Case AttendanceCodes.HOLIDAY
                    result.Holidays += 1D
                Case AttendanceCodes.CASUAL_LEAVE
                    result.CasualLeave += 1D
                Case AttendanceCodes.EARNED_LEAVE
                    result.EarnedLeave += 1D
                Case AttendanceCodes.SICK_LEAVE
                    result.SickLeave += 1D
                Case AttendanceCodes.COMP_OFF
                    result.CompLeave += 1D
            End Select
            result.OtHours += r.OtHours
        Next
        ApplyDerived(result, DaysInMonth(year, month))
        Return result
    End Function

    ' Default code for a blank day: holiday first, then weekly off, otherwise present.
    Public Shared Function SuggestStatus(day As Date, holidays As ICollection(Of Date), weeklyOff As DayOfWeek) As String
        If holidays IsNot Nothing AndAlso holidays.Contains(day.Date) Then
            Return AttendanceCodes.HOLIDAY
        End If
        If day.DayOfWeek = weeklyOff Then
            Return AttendanceCodes.WEEKLY_OFF
        End If
        Return AttendanceCodes.PRESENT
    End Function

    Private Shared Sub CheckValue(errors As List(Of String), label As String, value As Decimal, daysInMonth As Integer)
        If value < 0D Then
            errors.Add($"{label} cannot be negative.")
        ElseIf Not IsHalfStep(value) Then
            errors.Add($"{label} must be in steps of 0.5.")
        ElseIf value > daysInMonth Then
            errors.Add($"{label} cannot exceed {daysInMonth} days.")
        End If
    End Sub

    Private Shared Function IsValidClock(value As TimeSpan) As Boolean
        Return value >= TimeSpan.Zero AndAlso value < TimeSpan.FromHours(HOURS_PER_DAY)
    End Function

End Class
