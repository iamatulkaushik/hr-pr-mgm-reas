Public Class DailyAttendanceRow

    Public Property EmployeeId As Integer
    Public Property EmpCode As String = String.Empty
    Public Property EmployeeName As String = String.Empty
    Public Property AttDate As Date
    Public Property Status As String = String.Empty
    Public Property InTime As TimeSpan?
    Public Property OutTime As TimeSpan?
    Public Property OtHours As Decimal
    Public Property IsLocked As Boolean

End Class
