Imports Hrms.Models

Public Interface IAttendanceRepository

    Function GetDaily(companyId As Integer, day As Date) As List(Of DailyAttendanceRow)

    Function GetDailyForMonth(companyId As Integer, year As Integer, month As Integer) As List(Of DailyAttendanceRow)

    Function SaveDaily(companyId As Integer, day As Date, userId As Integer, rows As IList(Of DailyAttendanceRow)) As Integer

    Function GetMonthly(companyId As Integer, year As Integer, month As Integer) As List(Of MonthlyAttendanceRow)

    Function SaveMonthly(companyId As Integer, year As Integer, month As Integer, userId As Integer, rows As IList(Of MonthlyAttendanceRow)) As Integer

End Interface
