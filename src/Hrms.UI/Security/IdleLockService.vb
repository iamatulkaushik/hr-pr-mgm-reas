Imports System.Windows.Forms

''' <summary>
''' Locks the main window after 10 idle minutes or on Ctrl+L. While locked the main window is invisible
''' (opacity 0) so no data shows; frmUnlock must be passed. Log out from the lock screen raises LogoutRequested.
''' </summary>
Public NotInheritable Class IdleLockService
    Implements IMessageFilter, IDisposable

    Private Const WM_KEYDOWN As Integer = &H100
    Private Const WM_SYSKEYDOWN As Integer = &H104
    Private Const WM_MOUSEMOVE As Integer = &H200
    Private Const WM_LBUTTONDOWN As Integer = &H201
    Private Const WM_RBUTTONDOWN As Integer = &H204
    Private Const WM_MOUSEWHEEL As Integer = &H20A
    Private Const CHECK_INTERVAL_MS As Integer = 5000

    Private ReadOnly _owner As Form
    Private ReadOnly _monitor As IdleMonitor
    Private ReadOnly _timer As Timer
    Private _locked As Boolean

    Public Event LogoutRequested As EventHandler

    Public Sub New(owner As Form)
        _owner = owner
        _monitor = New IdleMonitor(TimeSpan.FromMinutes(IdleMonitor.DEFAULT_TIMEOUT_MINUTES), Function() DateTime.UtcNow)
        _timer = New Timer With {.Interval = CHECK_INTERVAL_MS}
        AddHandler _timer.Tick, AddressOf OnTick
    End Sub

    Public Sub Start()
        _monitor.Touch()
        Application.AddMessageFilter(Me)
        _timer.Start()
    End Sub

    Public Sub [Stop]()
        Application.RemoveMessageFilter(Me)
        _timer.Stop()
    End Sub

    Public Function PreFilterMessage(ByRef m As Message) As Boolean Implements IMessageFilter.PreFilterMessage
        If _locked Then Return False
        Select Case m.Msg
            Case WM_KEYDOWN
                If m.WParam.ToInt32() = CInt(Keys.L) AndAlso (Control.ModifierKeys And Keys.Control) = Keys.Control Then
                    _owner.BeginInvoke(New Action(AddressOf LockNow))
                    Return True
                End If
                _monitor.Touch()
            Case WM_SYSKEYDOWN, WM_MOUSEMOVE, WM_LBUTTONDOWN, WM_RBUTTONDOWN, WM_MOUSEWHEEL
                _monitor.Touch()
        End Select
        Return False
    End Function

    Private Sub OnTick(sender As Object, e As EventArgs)
        If _monitor.IsExpired() Then LockNow()
    End Sub

    Public Sub LockNow()
        If _locked OrElse Not AppSession.IsSignedIn Then Return
        _locked = True
        _timer.Stop()
        Dim previousOpacity As Double = _owner.Opacity
        Dim result As DialogResult = DialogResult.Abort
        Try
            _owner.Opacity = 0
            Using form As New frmUnlock(AppSession.User.UserName)
                result = form.ShowDialog(_owner)
            End Using
        Finally
            _owner.Opacity = previousOpacity
            _locked = False
            _monitor.Touch()
            _timer.Start()
        End Try
        If result <> DialogResult.OK Then RaiseEvent LogoutRequested(Me, EventArgs.Empty)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        [Stop]()
        _timer.Dispose()
    End Sub

End Class
