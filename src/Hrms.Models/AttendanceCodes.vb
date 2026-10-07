Public NotInheritable Class AttendanceCodes

    Public Const PRESENT As String = "P"
    Public Const ABSENT As String = "A"
    Public Const HALF_DAY As String = "HD"
    Public Const HOLIDAY As String = "H"
    Public Const WEEKLY_OFF As String = "WO"
    Public Const CASUAL_LEAVE As String = "CL"
    Public Const EARNED_LEAVE As String = "EL"
    Public Const SICK_LEAVE As String = "SL"
    Public Const COMP_OFF As String = "CO"

    Public Shared ReadOnly All As String() = {PRESENT, ABSENT, HALF_DAY, HOLIDAY, WEEKLY_OFF, CASUAL_LEAVE, EARNED_LEAVE, SICK_LEAVE, COMP_OFF}

    Private Sub New()
    End Sub

    Public Shared Function Normalize(code As String) As String
        If code Is Nothing Then
            Return String.Empty
        End If
        Return code.Trim().ToUpperInvariant()
    End Function

    Public Shared Function IsValid(code As String) As Boolean
        Return Array.IndexOf(All, Normalize(code)) >= 0
    End Function

End Class
