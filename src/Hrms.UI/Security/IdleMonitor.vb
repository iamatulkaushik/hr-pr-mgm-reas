''' <summary>Pure idle-time logic (clock injected so it can be tested).</summary>
Public NotInheritable Class IdleMonitor

    Public Const DEFAULT_TIMEOUT_MINUTES As Integer = 10

    Private ReadOnly _timeout As TimeSpan
    Private ReadOnly _clock As Func(Of DateTime)
    Private _lastActivity As DateTime

    Public Sub New(timeout As TimeSpan, clock As Func(Of DateTime))
        _timeout = timeout
        _clock = clock
        _lastActivity = clock()
    End Sub

    Public Sub Touch()
        _lastActivity = _clock()
    End Sub

    Public Function IsExpired() As Boolean
        Return _clock() - _lastActivity >= _timeout
    End Function

End Class
