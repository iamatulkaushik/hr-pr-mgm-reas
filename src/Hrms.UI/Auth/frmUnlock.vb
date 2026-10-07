Imports Hrms.Models.Auth

''' <summary>
''' Idle-lock screen. DialogResult.OK = unlocked, DialogResult.Abort = log out.
''' Cannot be closed any other way (no Esc, no Alt+F4).
''' </summary>
Public Class frmUnlock

    Private ReadOnly _userName As String

    Public Sub New(userName As String)
        InitializeComponent()
        _userName = userName
    End Sub

    Private Sub frmUnlock_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        lblUser.Text = "Signed in as " & _userName
        lblMessage.Text = String.Empty
        txtPassword.Select()
    End Sub

    Protected Overrides Sub CancelRequested()
        ' Esc must not unlock or close the screen.
    End Sub

    Private Sub frmUnlock_FormClosing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        If DialogResult = DialogResult.None Then e.Cancel = True
    End Sub

    Private Sub btnLogOut_Click(sender As Object, e As EventArgs) Handles btnLogOut.Click
        DialogResult = DialogResult.Abort
    End Sub

    Private Sub btnUnlock_Click(sender As Object, e As EventArgs) Handles btnUnlock.Click
        If txtPassword.Text.Length = 0 Then
            lblMessage.Text = "Enter the password."
            txtPassword.Select()
            Return
        End If

        Dim result As LoginResult = Nothing
        If Not RunGuarded(Sub() result = AppServices.Auth.Unlock(_userName, txtPassword.Text)) Then
            txtPassword.Clear()
            Return
        End If
        txtPassword.Clear()

        Select Case result.Status
            Case LoginStatus.Success
                DialogResult = DialogResult.OK
            Case LoginStatus.Locked, LoginStatus.Inactive
                Msg.Warn(Me, "This account is now locked. You will be signed out.")
                DialogResult = DialogResult.Abort
            Case Else
                lblMessage.Text = "Wrong password."
                txtPassword.Select()
        End Select
    End Sub

End Class
