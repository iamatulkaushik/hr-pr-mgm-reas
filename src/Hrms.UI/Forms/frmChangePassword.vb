Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

''' <summary>
''' Change-password dialog. With forced = True (first login) the user must change the password to continue:
''' the cancel button then reads "Sign out" and the caller treats Cancel as not signed in.
''' The password rules are checked in the BLL. This form only checks that the confirmation matches.
''' </summary>
Friend Class frmChangePassword
    Inherits BaseForm

    Private ReadOnly _auth As IAuthService
    Private ReadOnly _session As UserSession
    Private ReadOnly _txtCurrent As New TextBox() With {.UseSystemPasswordChar = True}
    Private ReadOnly _txtNew As New TextBox() With {.UseSystemPasswordChar = True}
    Private ReadOnly _txtConfirm As New TextBox() With {.UseSystemPasswordChar = True}
    Private ReadOnly _lblMessage As Label = DialogLayout.CreateMessageLabel()
    Private ReadOnly _btnChange As New Button() With {.Text = "Change password"}
    Private ReadOnly _btnCancel As New Button() With {.DialogResult = DialogResult.Cancel}

    Public Sub New(auth As IAuthService, session As UserSession, forced As Boolean)
        _auth = auth
        _session = session

        Text = If(forced, "Change password (required)", "Change password")
        _btnCancel.Text = If(forced, "Sign out", "Cancel")
        DialogLayout.StyleDialog(Me, 420, If(forced, 360, 320))
        CancelButton = _btnCancel
        AddHandler _btnChange.Click, Sub(sender, e) PerformChange()

        Dim grid = DialogLayout.CreateGrid()
        If forced Then
            DialogLayout.AddFullRow(grid, DialogLayout.CreateHintLabel("You must choose a new password before you continue."))
        End If
        DialogLayout.AddField(grid, "Current password", _txtCurrent)
        DialogLayout.AddField(grid, "New password", _txtNew)
        DialogLayout.AddField(grid, "Confirm new password", _txtConfirm)
        DialogLayout.AddFullRow(grid, _lblMessage)
        DialogLayout.AddFullRow(grid, DialogLayout.CreateButtonBar(_btnChange, _btnCancel))
        Controls.Add(grid)
        Controls.Add(DialogLayout.CreateHeader("Change password"))
    End Sub

    Protected Overrides Sub OnShown(e As EventArgs)
        MyBase.OnShown(e)
        _txtCurrent.Focus()
    End Sub

    Protected Overrides Function HandleEnterKey(active As Control) As Boolean
        If active Is _txtConfirm Then
            PerformChange()
            Return True
        End If
        Return MyBase.HandleEnterKey(active)
    End Function

    Private Sub PerformChange()
        _lblMessage.Text = String.Empty
        If _txtCurrent.Text.Length = 0 OrElse _txtNew.Text.Length = 0 Then
            _lblMessage.Text = "Fill in the current and the new password."
            Return
        End If
        If _txtNew.Text <> _txtConfirm.Text Then
            _lblMessage.Text = "The new password and the confirmation do not match."
            _txtConfirm.Clear()
            _txtConfirm.Focus()
            Return
        End If

        Dim result As PasswordChangeResult = Nothing
        UseWaitCursor = True
        Try
            result = _auth.ChangePassword(_session, _txtCurrent.Text, _txtNew.Text, Environment.MachineName)
        Finally
            UseWaitCursor = False
        End Try

        Select Case result.Outcome
            Case PasswordChangeOutcome.Changed
                ClearFields()
                DialogResult = DialogResult.OK
            Case PasswordChangeOutcome.WrongCurrentPassword
                _lblMessage.Text = "The current password is not correct."
                _txtCurrent.Clear()
                _txtCurrent.Focus()
            Case Else
                _lblMessage.Text = result.Message
                _txtNew.Focus()
        End Select
    End Sub

    Private Sub ClearFields()
        _txtCurrent.Clear()
        _txtNew.Clear()
        _txtConfirm.Clear()
    End Sub

End Class
