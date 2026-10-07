Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmUnlock
    Inherits BaseForm

    <System.Diagnostics.DebuggerNonUserCode()>
    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private components As System.ComponentModel.IContainer

    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Me.lblTitle = New Label()
        Me.lblUser = New Label()
        Me.lblPassword = New Label()
        Me.txtPassword = New TextBox()
        Me.lblMessage = New Label()
        Me.btnUnlock = New Button()
        Me.btnLogOut = New Button()
        Me.SuspendLayout()
        '
        'lblTitle
        '
        Me.lblTitle.AutoSize = True
        Me.lblTitle.Font = New Font("Segoe UI", 13.0F, FontStyle.Bold)
        Me.lblTitle.ForeColor = Color.FromArgb(31, 78, 121)
        Me.lblTitle.Location = New Point(24, 18)
        Me.lblTitle.Name = "lblTitle"
        Me.lblTitle.Text = "Session locked"
        '
        'lblUser
        '
        Me.lblUser.AutoSize = True
        Me.lblUser.Location = New Point(26, 52)
        Me.lblUser.Name = "lblUser"
        '
        'lblPassword
        '
        Me.lblPassword.AutoSize = True
        Me.lblPassword.Location = New Point(26, 86)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Text = "Password"
        '
        'txtPassword
        '
        Me.txtPassword.Location = New Point(26, 108)
        Me.txtPassword.MaxLength = 64
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.Size = New Size(328, 25)
        Me.txtPassword.TabIndex = 0
        Me.txtPassword.UseSystemPasswordChar = True
        '
        'lblMessage
        '
        Me.lblMessage.ForeColor = Color.FromArgb(178, 34, 34)
        Me.lblMessage.Location = New Point(26, 138)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.Size = New Size(328, 36)
        '
        'btnUnlock
        '
        Me.btnUnlock.Location = New Point(144, 182)
        Me.btnUnlock.Name = "btnUnlock"
        Me.btnUnlock.Size = New Size(100, 34)
        Me.btnUnlock.TabIndex = 1
        Me.btnUnlock.Tag = "primary"
        Me.btnUnlock.Text = "Unlock"
        '
        'btnLogOut
        '
        Me.btnLogOut.Location = New Point(252, 182)
        Me.btnLogOut.Name = "btnLogOut"
        Me.btnLogOut.Size = New Size(102, 34)
        Me.btnLogOut.TabIndex = 2
        Me.btnLogOut.Text = "Log out"
        '
        'frmUnlock
        '
        Me.AcceptButton = Me.btnUnlock
        Me.AutoScaleDimensions = New SizeF(7.0F, 17.0F)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.ClientSize = New Size(380, 236)
        Me.ControlBox = False
        Me.Controls.Add(Me.btnLogOut)
        Me.Controls.Add(Me.btnUnlock)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.lblPassword)
        Me.Controls.Add(Me.lblUser)
        Me.Controls.Add(Me.lblTitle)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.Name = "frmUnlock"
        Me.ShowInTaskbar = False
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Text = "Revolution HRMS - Locked"
        Me.TopMost = True
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblTitle As Label
    Friend WithEvents lblUser As Label
    Friend WithEvents lblPassword As Label
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents lblMessage As Label
    Friend WithEvents btnUnlock As Button
    Friend WithEvents btnLogOut As Button

End Class
