Public Class MonthlyAttendanceRow

    Public Property EmployeeId As Integer
    Public Property EmpCode As String = String.Empty
    Public Property EmployeeName As String = String.Empty
    Public Property Year As Integer
    Public Property Month As Integer
    Public Property DaysWorked As Decimal
    Public Property WeeklyOff As Decimal
    Public Property Holidays As Decimal
    Public Property CasualLeave As Decimal
    Public Property EarnedLeave As Decimal
    Public Property SickLeave As Decimal
    Public Property CompLeave As Decimal
    Public Property Absent As Decimal
    Public Property OtHours As Decimal
    Public Property DaysPaid As Decimal
    Public Property IsLocked As Boolean
    Public Property HasRecord As Boolean

End Class
