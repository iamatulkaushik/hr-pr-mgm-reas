Imports Hrms.Models.Auth

''' <summary>Sign-in screen (FR-1). Returns the signed-in session on DialogResult.OK.</summary>
Public Class frmLogin

    Private _session As UserSession

    Public ReadOnly Property Session As UserSession
        Get
            Return _session
        End Get
    End Property

    Private Sub frmLogin_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MarkMandatory(lblUserName, lblPassword)
        lblMessage.Text = String.Empty
        lblVersion.Text = "v" & Application.ProductVersion
        txtUserName.Select()
    End Sub

    Private Sub btnLogin_Click(sender As Object, e As EventArgs) Handles btnLogin.Click
        SignIn()
    End Sub

    Protected Overrides Sub SaveRequested()
        SignIn()
    End Sub

    Private Sub txtPassword_Enter(sender As Object, e As EventArgs) Handles txtPassword.Enter, txtPassword.KeyUp
        lblCaps.Visible = IsKeyLocked(Keys.CapsLock)
    End Sub

    Private Sub SignIn()
        lblMessage.Text = String.Empty
        Dim userName As String = txtUserName.Text.Trim()
        If userName.Length = 0 Then
            ShowProblem("Enter the user name.", txtUserName)
            Return
        End If
        If txtPassword.Text.Length = 0 Then
            ShowProblem("Enter the password.", txtPassword)
            Return
        End If

        Dim result As LoginResult = Nothing
        If Not RunGuarded(Sub() result = AppServices.Auth.Login(userName, txtPassword.Text)) Then
            txtPassword.Clear()
            Return
        End If

        If result.Status = LoginStatus.Success Then
            _session = result.Session
            txtPassword.Clear()
            DialogResult = DialogResult.OK
            Return
        End If

        txtPassword.Clear()
        ShowProblem(MessageFor(result), txtPassword)
    End Sub

    ''' <summary>Same text for unknown user and wrong password, so a user name cannot be probed.</summary>
    Private Shared Function MessageFor(result As LoginResult) As String
        Select Case result.Status
            Case LoginStatus.Locked
                If result.LockedUntil.HasValue Then
                    Return "This account is locked until " & result.LockedUntil.Value.ToString("dd-MM-yyyy HH:mm") &
                           ". Contact your Associate."
                End If
                Return "This account is locked. Contact your Associate."
            Case LoginStatus.Inactive
                Return "This account is disabled. Contact your Associate."
            Case Else
                If result.AttemptsRemaining.HasValue Then
                    Return "Invalid user name or password. Attempts left: " & result.AttemptsRemaining.Value & "."
                End If
                Return "Invalid user name or password."
        End Select
    End Function

    Private Sub ShowProblem(text As String, focusOn As Control)
        lblMessage.Text = text
        focusOn.Select()
    End Sub

End Class
