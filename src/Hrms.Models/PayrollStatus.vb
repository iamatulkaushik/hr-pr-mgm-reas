''' <summary>Payroll run states. A Locked run changes only by a reversal entry.</summary>
Public Enum PayrollStatus
    Draft = 1
    Processed = 2
    Approved = 3
    Locked = 4
End Enum
