Public Class OvertimeSettings

    Public Const DEFAULT_SHIFT_HOURS As Decimal = 8D
    Public Const DEFAULT_STEP_HOURS As Decimal = 0.5D

    Public Property ShiftHours As Decimal = DEFAULT_SHIFT_HOURS
    Public Property Rounding As OvertimeRounding = OvertimeRounding.None
    Public Property StepHours As Decimal = DEFAULT_STEP_HOURS

End Class
