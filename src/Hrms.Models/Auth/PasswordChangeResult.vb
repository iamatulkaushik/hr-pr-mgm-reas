''' <summary>Result of a password change. Message explains a PolicyViolation to the user.</summary>
Public NotInheritable Class PasswordChangeResult

    Private Sub New(outcome As PasswordChangeOutcome, message As String)
        Me.Outcome = outcome
        Me.Message = message
    End Sub

    Public ReadOnly Property Outcome As PasswordChangeOutcome
    Public ReadOnly Property Message As String

    Public Shared Function Changed() As PasswordChangeResult
        Return New PasswordChangeResult(PasswordChangeOutcome.Changed, String.Empty)
    End Function

    Public Shared Function WrongCurrent() As PasswordChangeResult
        Return New PasswordChangeResult(PasswordChangeOutcome.WrongCurrentPassword, String.Empty)
    End Function

    Public Shared Function Rejected(message As String) As PasswordChangeResult
        Return New PasswordChangeResult(PasswordChangeOutcome.PolicyViolation, message)
    End Function

End Class
