Imports System.Drawing
Imports System.Windows.Forms
Imports Hrms.Models
Imports Hrms.BLL

''' <summary>
''' Full-screen lock shown after 10 idle minutes or Ctrl+L. It hides the application until the signed-in
''' user's password is entered. DialogResult.OK = unlocked, DialogResult.Abort = sign out
''' (the Sign out button, or too many wrong passwords). It cannot be closed any other way.
''' </summary>
Friend Class frmUnlock
    Inherits Form

    Private Const MAX_ATTEMPTS As Integer = 5

    Private ReadOnly _auth As IAuthService
    Private ReadOnly _session As UserSession
    Private ReadOnly _panel As New Panel() With {.Width = 420, .Height = 270, .BackColor = Color.White}
    Private ReadOnly _txtPassword As New TextBox() With {.UseSystemPasswordChar = True}
    Private ReadOnly _lblMessage As Label = DialogLayout.CreateMessageLabel()
    Private ReadOnly _btnUnlock As New Button() With {.Text = "Unlock"}
    Private ReadOnly _btnSignOut As New Button() With {.Text = "Sign out"}
    Private _attempts As Integer

    Public Sub New(auth As IAuthService, session As UserSession)
        _auth = auth
        _session = session

        Text = "Revolution HRMS - Locked"
        FormBorderStyle = FormBorderStyle.None
        WindowState = FormWindowState.Maximized
        TopMost = True
        ShowInTaskbar = False
        BackColor = AppTheme.Primary
        Font = AppTheme.BaseFont
        AcceptButton = _btnUnlock

        AddHandler _btnUnlock.Click, Sub(sender, e) TryUnlock()
        AddHandler _btnSignOut.Click, Sub(sender, e) DialogResult = DialogResult.Abort
        AddHandler Me.Resize, Sub(sender, e) CenterPanel()

        Dim grid = DialogLayout.CreateGrid()
        DialogLayout.AddFullRow(grid, DialogLayout.CreateHintLabel("Signed in as " & session.Username & ". Enter your password to continue."))
        DialogLayout.AddField(grid, "Password", _txtPassword)
        DialogLayout.AddFullRow(grid, _lblMessage)
        DialogLayout.AddFullRow(grid, DialogLayout.CreateButtonBar(_btnUnlock, _btnSignOut))
        _panel.Controls.Add(grid)
        _panel.Controls.Add(DialogLayout.CreateHeader("Session locked"))
        Controls.Add(_panel)
        CenterPanel()
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        CenterPanel()
        Activate()
        _txtPassword.Focus()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        ' Alt+F4 and similar must not skip the lock. Windows shutdown is allowed through.
        If DialogResult = DialogResult.None AndAlso e.CloseReason = CloseReason.UserClosing Then
            e.Cancel = True
        End If
        MyBase.OnFormClosing(e)
    End Sub

    Private Sub CenterPanel()
        _panel.Location = New Point((ClientSize.Width - _panel.Width) \ 2, (ClientSize.Height - _panel.Height) \ 2)
    End Sub

    Private Sub TryUnlock()
        If _auth.VerifyPassword(_session, _txtPassword.Text) Then
            _txtPassword.Clear()
            DialogResult = DialogResult.OK
            Return
        End If

        _attempts += 1
        _txtPassword.Clear()
        If _attempts >= MAX_ATTEMPTS Then
            DialogResult = DialogResult.Abort
            Return
        End If
        _lblMessage.Text = $"Wrong password. {MAX_ATTEMPTS - _attempts} attempt(s) left before you are signed out."
        _txtPassword.Focus()
    End Sub

End Class
