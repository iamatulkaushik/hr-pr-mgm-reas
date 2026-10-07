''' <summary>Result of a sign-in attempt. Session is set only when Outcome is Success.</summary>
Public NotInheritable Class LoginResult

    Private Sub New(outcome As LoginOutcome, session As UserSession, lockedUntilUtc As Date?)
        Me.Outcome = outcome
        Me.Session = session
        Me.LockedUntilUtc = lockedUntilUtc
    End Sub

    Public ReadOnly Property Outcome As LoginOutcome
    Public ReadOnly Property Session As UserSession

    ''' <summary>Set when Outcome is AccountLocked and the end of the lock is known. UTC.</summary>
    Public ReadOnly Property LockedUntilUtc As Date?

    Public Shared Function Succeeded(session As UserSession) As LoginResult
        Return New LoginResult(LoginOutcome.Success, session, Nothing)
    End Function

    Public Shared Function Failed(outcome As LoginOutcome, Optional lockedUntilUtc As Date? = Nothing) As LoginResult
        Return New LoginResult(outcome, Nothing, lockedUntilUtc)
    End Function

End Class
