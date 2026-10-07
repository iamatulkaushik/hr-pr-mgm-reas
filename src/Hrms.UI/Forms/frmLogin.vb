Imports System.Globalization
Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

''' <summary>
''' Sign-in dialog. Enter in the user name field moves to the password, Enter in the password signs in.
''' The lockout rule (5 failed logins) lives in the BLL. This form only shows the outcome.
''' </summary>
Friend Class frmLogin
    Inherits BaseForm

    Private ReadOnly _auth As IAuthService
    Private ReadOnly _txtUser As New TextBox()
    Private ReadOnly _txtPassword As New TextBox() With {.UseSystemPasswordChar = True}
    Private ReadOnly _lblMessage As Label = DialogLayout.CreateMessageLabel()
    Private ReadOnly _btnLogin As New Button() With {.Text = "Sign in"}
    Private ReadOnly _btnExit As New Button() With {.Text = "Exit", .DialogResult = DialogResult.Cancel}
    Private _session As UserSession

    Public Sub New(auth As IAuthService)
        _auth = auth

        Text = "Revolution HRMS - Sign in"
        StartPosition = FormStartPosition.CenterScreen
        DialogLayout.StyleDialog(Me, 400, 290)
        CancelButton = _btnExit
        AddHandler _btnLogin.Click, Sub(sender, e) PerformLogin()

        Dim grid = DialogLayout.CreateGrid()
        DialogLayout.AddField(grid, "User name", _txtUser)
        DialogLayout.AddField(grid, "Password", _txtPassword)
        DialogLayout.AddFullRow(grid, _lblMessage)
        DialogLayout.AddFullRow(grid, DialogLayout.CreateButtonBar(_btnLogin, _btnExit))
        Controls.Add(grid)
        Controls.Add(DialogLayout.CreateHeader("Revolution HRMS"))
    End Sub

    ''' <summary>Set after a successful sign-in.</summary>
    Public ReadOnly Property Session As UserSession
        Get
            Return _session
        End Get
    End Property

    ''' <summary>Clears the fields before the dialog is shown again.</summary>
    Public Sub ResetFields()
        _session = Nothing
        _txtPassword.Clear()
        _lblMessage.Text = String.Empty
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        If _txtUser.Text.Length = 0 Then
            _txtUser.Focus()
        Else
            _txtPassword.Focus()
        End If
    End Sub

    Protected Overrides Function HandleEnterKey(active As Control) As Boolean
        If active Is _txtPassword Then
            PerformLogin()
            Return True
        End If
        Return MyBase.HandleEnterKey(active)
    End Function

    Private Sub PerformLogin()
        _lblMessage.Text = String.Empty
        Dim username = _txtUser.Text.Trim()
        If username.Length = 0 OrElse _txtPassword.Text.Length = 0 Then
            _lblMessage.Text = "Enter your user name and password."
            Return
        End If

        Dim result As LoginResult = Nothing
        UseWaitCursor = True
        Try
            result = _auth.Login(username, _txtPassword.Text, Environment.MachineName)
        Finally
            UseWaitCursor = False
        End Try
        _txtPassword.Clear()

        Select Case result.Outcome
            Case LoginOutcome.Success
                _session = result.Session
                DialogResult = DialogResult.OK
                Return
            Case LoginOutcome.AccountLocked
                _lblMessage.Text = LockedText(result.LockedUntilUtc)
            Case LoginOutcome.AccountInactive
                _lblMessage.Text = "This account is inactive. Contact your administrator."
            Case Else
                _lblMessage.Text = "Invalid user name or password."
        End Select
        _txtPassword.Focus()
    End Sub

    Private Shared Function LockedText(lockedUntilUtc As Date?) As String
        If lockedUntilUtc.HasValue Then
            Dim localTime = Date.SpecifyKind(lockedUntilUtc.Value, DateTimeKind.Utc).ToLocalTime()
            Return "This account is locked until " & localTime.ToString("HH:mm", CultureInfo.InvariantCulture) &
                   ". Try again then, or contact your administrator."
        End If
        Return "This account is locked after too many failed attempts. Try again later, or contact your administrator."
    End Function

End Class
