Imports Hrms.Models
Imports Hrms.Models.Auth

''' <summary>
''' Change password (FR-2). With forced = True (first login, MustChangePwd) the user cannot skip it:
''' the cancel button becomes "Log out" and the caller signs the user out on DialogResult.Cancel.
''' </summary>
Public Class frmChangePassword

    Private ReadOnly _session As UserSession
    Private ReadOnly _forced As Boolean

    Public Sub New(session As UserSession, forced As Boolean)
        InitializeComponent()
        _session = session
        _forced = forced
    End Sub

    Private Sub frmChangePassword_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MarkMandatory(lblOld, lblNew, lblConfirm)
        lblMessage.Text = String.Empty
        lblHint.Text = $"At least {PasswordPolicy.MIN_LENGTH} characters with a capital letter, a small letter and a digit."
        If _forced Then
            lblIntro.Text = "You must set a new password before you continue."
            btnCancel.Text = "Log out"
            ControlBox = False
        Else
            lblIntro.Text = "Enter your current password and choose a new one."
        End If
        txtOld.Select()
    End Sub

    Protected Overrides Sub SaveRequested()
        SavePassword()
    End Sub

    Private Sub btnSave_Click(sender As Object, e As EventArgs) Handles btnSave.Click
        SavePassword()
    End Sub

    Private Sub SavePassword()
        lblMessage.Text = String.Empty
        If txtOld.Text.Length = 0 Then
            ShowProblem("Enter the current password.", txtOld)
            Return
        End If
        If txtNew.Text <> txtConfirm.Text Then
            ShowProblem("New password and confirmation do not match.", txtConfirm)
            Return
        End If
        Dim policy As OperationResult = PasswordPolicy.Validate(txtNew.Text, txtOld.Text, _session.UserName)
        If Not policy.Succeeded Then
            ShowProblem(policy.Message, txtNew)
            Return
        End If

        Dim result As OperationResult = Nothing
        If Not RunGuarded(Sub() result = AppServices.Auth.ChangePassword(_session.UserId, txtOld.Text, txtNew.Text)) Then
            Return
        End If
        If Not result.Succeeded Then
            ShowProblem(result.Message, txtOld)
            Return
        End If

        _session.MustChangePassword = False
        ClearFields()
        Msg.Success("Password changed.")
        DialogResult = DialogResult.OK
    End Sub

    Private Sub ClearFields()
        txtOld.Clear()
        txtNew.Clear()
        txtConfirm.Clear()
    End Sub

    Private Sub ShowProblem(text As String, focusOn As Control)
        lblMessage.Text = text
        focusOn.Select()
    End Sub

End Class
