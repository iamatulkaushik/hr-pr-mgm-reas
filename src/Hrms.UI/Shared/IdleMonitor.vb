Imports System.Drawing
Imports System.Windows.Forms
Imports Hrms.Models

''' <summary>
''' Watches keyboard and mouse input for this application and raises IdleTimeout when nothing happened for the
''' configured time. Call Suspend while the session is locked and Resume after unlocking.
''' </summary>
Friend NotInheritable Class IdleMonitor
    Implements IMessageFilter
    Implements IDisposable

    Private Const WM_KEYDOWN As Integer = &H100
    Private Const WM_MOUSEMOVE As Integer = &H200
    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const WM_RBUTTONDOWN As Integer = &H204
    Private Const WM_MBUTTONDOWN As Integer = &H207
    Private Const WM_MOUSEWHEEL As Integer = &H20A
    Private Const CHECK_INTERVAL_MS As Integer = 5000

    Private ReadOnly _tracker As IdleTracker
    Private ReadOnly _timer As New System.Windows.Forms.Timer()
    Private _lastMouse As Point
    Private _suspended As Boolean
    Private _disposed As Boolean

    Public Event IdleTimeout As EventHandler

    Public Sub New(timeout As TimeSpan)
        _tracker = New IdleTracker(timeout)
        _lastMouse = Control.MousePosition
        _timer.Interval = CHECK_INTERVAL_MS
        AddHandler _timer.Tick, AddressOf OnTick
        Application.AddMessageFilter(Me)
        _timer.Start()
    End Sub

    Public Sub Suspend()
        _suspended = True
    End Sub

    Public Sub [Resume]()
        _tracker.Touch()
        _suspended = False
    End Sub

    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        Select Case m.Msg
            Case WM_KEYDOWN, WM_LBUTTONDOWN, WM_RBUTTONDOWN, WM_MBUTTONDOWN, WM_MOUSEWHEEL
                _tracker.Touch()
            Case WM_MOUSEMOVE
                ' Windows sends mouse-move messages even when the mouse is still, so only a real move counts.
                Dim position = Control.MousePosition
                If position <> _lastMouse Then
                    _lastMouse = position
                    _tracker.Touch()
                End If
        End Select
        Return False
    End Function

    Private Sub OnTick(sender As Object, e As EventArgs)
        If _suspended Then Return
        If _tracker.IsIdle() Then
            _suspended = True
            RaiseEvent IdleTimeout(Me, EventArgs.Empty)
        End If
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        If _disposed Then Return
        _disposed = True
        Application.RemoveMessageFilter(Me)
        _timer.Stop()
        _timer.Dispose()
    End Sub

End Class
