Imports Revolution.Hrms.Models
Imports Xunit

Public Class IdleTrackerTests

    Private ReadOnly _start As Date = New Date(2026, 10, 1, 10, 0, 0, DateTimeKind.Utc)
    Private _current As Date

    Public Sub New()
        _current = _start
    End Sub

    Private Function CreateTracker() As IdleTracker
        Return New IdleTracker(TimeSpan.FromMinutes(10), Function() _current)
    End Function

    <Fact>
    Public Sub JustStarted_IsNotIdle()
        Assert.False(CreateTracker().IsIdle())
    End Sub

    <Fact>
    Public Sub NineMinutesFiftyNine_IsNotIdle()
        Dim tracker = CreateTracker()
        _current = _start.AddMinutes(9).AddSeconds(59)
        Assert.False(tracker.IsIdle())
    End Sub

    <Fact>
    Public Sub TenMinutes_IsIdle()
        Dim tracker = CreateTracker()
        _current = _start.AddMinutes(10)
        Assert.True(tracker.IsIdle())
    End Sub

    <Fact>
    Public Sub Touch_RestartsTheTimeout()
        Dim tracker = CreateTracker()
        _current = _start.AddMinutes(9)
        tracker.Touch()
        _current = _start.AddMinutes(18)
        Assert.False(tracker.IsIdle())
        _current = _start.AddMinutes(19)
        Assert.True(tracker.IsIdle())
    End Sub

    <Fact>
    Public Sub ZeroTimeout_Throws()
        Assert.Throws(Of ArgumentOutOfRangeException)(Function() New IdleTracker(TimeSpan.Zero))
    End Sub

End Class
