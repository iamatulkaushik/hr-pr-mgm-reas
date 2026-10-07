Imports Hrms.Models

Public Interface IMonthCloseRepository

    Function GetStatus(companyId As Integer, year As Integer, month As Integer) As MonthCloseStatus

    Function CountMissingMonthly(companyId As Integer, year As Integer, month As Integer) As Integer

    Function GetLog(companyId As Integer, year As Integer, month As Integer) As List(Of MonthCloseLogEntry)

    Sub CloseMonth(companyId As Integer, year As Integer, month As Integer, userId As Integer)

    Sub ReopenMonth(companyId As Integer, year As Integer, month As Integer, userId As Integer, reason As String)

End Interface
