''' <summary>
''' Pure idle-time logic for the auto-lock (docs/ACCESS_CONTROL.md: 10 minutes). The UI calls Touch on input
''' and asks IsIdle on a timer. The clock can be replaced so the logic can be tested.
''' </summary>
Public NotInheritable Class IdleTracker

    Private ReadOnly _clock As Func(Of Date)
    Private _lastActivity As Date

    Public Sub New(timeout As TimeSpan, Optional clock As Func(Of Date) = Nothing)
        If timeout <= TimeSpan.Zero Then
            Throw New ArgumentOutOfRangeException(NameOf(timeout), "The idle timeout must be longer than zero.")
        End If
        Me.Timeout = timeout
        If clock Is Nothing Then
            _clock = Function() Date.UtcNow
        Else
            _clock = clock
        End If
        _lastActivity = _clock()
    End Sub

    Public ReadOnly Property Timeout As TimeSpan

    Public Sub Touch()
        _lastActivity = _clock()
    End Sub

    Public Function IsIdle() As Boolean
        Return _clock() - _lastActivity >= Timeout
    End Function

End Class
