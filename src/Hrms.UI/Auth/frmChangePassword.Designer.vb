Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmChangePassword
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
        Me.lblIntro = New Label()
        Me.lblOld = New Label()
        Me.txtOld = New TextBox()
        Me.lblNew = New Label()
        Me.txtNew = New TextBox()
        Me.lblConfirm = New Label()
        Me.txtConfirm = New TextBox()
        Me.lblHint = New Label()
        Me.lblMessage = New Label()
        Me.pnlFooter = New Panel()
        Me.btnSave = New Button()
        Me.btnCancel = New Button()
        Me.pnlFooter.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblIntro
        '
        Me.lblIntro.Location = New Point(24, 16)
        Me.lblIntro.Name = "lblIntro"
        Me.lblIntro.Size = New Size(372, 40)
        '
        'lblOld
        '
        Me.lblOld.AutoSize = True
        Me.lblOld.Location = New Point(24, 66)
        Me.lblOld.Name = "lblOld"
        Me.lblOld.Text = "Current password"
        '
        'txtOld
        '
        Me.txtOld.Location = New Point(24, 88)
        Me.txtOld.MaxLength = 64
        Me.txtOld.Name = "txtOld"
        Me.txtOld.Size = New Size(372, 25)
        Me.txtOld.TabIndex = 0
        Me.txtOld.UseSystemPasswordChar = True
        '
        'lblNew
        '
        Me.lblNew.AutoSize = True
        Me.lblNew.Location = New Point(24, 124)
        Me.lblNew.Name = "lblNew"
        Me.lblNew.Text = "New password"
        '
        'txtNew
        '
        Me.txtNew.Location = New Point(24, 146)
        Me.txtNew.MaxLength = 64
        Me.txtNew.Name = "txtNew"
        Me.txtNew.Size = New Size(372, 25)
        Me.txtNew.TabIndex = 1
        Me.txtNew.UseSystemPasswordChar = True
        '
        'lblConfirm
        '
        Me.lblConfirm.AutoSize = True
        Me.lblConfirm.Location = New Point(24, 182)
        Me.lblConfirm.Name = "lblConfirm"
        Me.lblConfirm.Text = "Confirm new password"
        '
        'txtConfirm
        '
        Me.txtConfirm.Location = New Point(24, 204)
        Me.txtConfirm.MaxLength = 64
        Me.txtConfirm.Name = "txtConfirm"
        Me.txtConfirm.Size = New Size(372, 25)
        Me.txtConfirm.TabIndex = 2
        Me.txtConfirm.UseSystemPasswordChar = True
        '
        'lblHint
        '
        Me.lblHint.ForeColor = Color.DimGray
        Me.lblHint.Location = New Point(24, 236)
        Me.lblHint.Name = "lblHint"
        Me.lblHint.Size = New Size(372, 36)
        '
        'lblMessage
        '
        Me.lblMessage.ForeColor = Color.FromArgb(178, 34, 34)
        Me.lblMessage.Location = New Point(24, 276)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.Size = New Size(372, 40)
        '
        'pnlFooter
        '
        Me.pnlFooter.Controls.Add(Me.btnSave)
        Me.pnlFooter.Controls.Add(Me.btnCancel)
        Me.pnlFooter.Dock = DockStyle.Bottom
        Me.pnlFooter.Name = "pnlFooter"
        Me.pnlFooter.Size = New Size(420, 60)
        Me.pnlFooter.TabStop = False
        '
        'btnSave
        '
        Me.btnSave.Location = New Point(196, 13)
        Me.btnSave.Name = "btnSave"
        Me.btnSave.Size = New Size(100, 34)
        Me.btnSave.TabIndex = 3
        Me.btnSave.Tag = "accent"
        Me.btnSave.Text = "Save"
        '
        'btnCancel
        '
        Me.btnCancel.DialogResult = DialogResult.Cancel
        Me.btnCancel.Location = New Point(304, 13)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New Size(92, 34)
        Me.btnCancel.TabIndex = 4
        Me.btnCancel.Text = "Cancel"
        '
        'frmChangePassword
        '
        Me.AcceptButton = Me.btnSave
        Me.AutoScaleDimensions = New SizeF(7.0F, 17.0F)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.CancelButton = Me.btnCancel
        Me.ClientSize = New Size(420, 380)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.lblHint)
        Me.Controls.Add(Me.txtConfirm)
        Me.Controls.Add(Me.lblConfirm)
        Me.Controls.Add(Me.txtNew)
        Me.Controls.Add(Me.lblNew)
        Me.Controls.Add(Me.txtOld)
        Me.Controls.Add(Me.lblOld)
        Me.Controls.Add(Me.lblIntro)
        Me.Controls.Add(Me.pnlFooter)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmChangePassword"
        Me.ShowInTaskbar = False
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Text = "Change password"
        Me.pnlFooter.ResumeLayout(False)
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents lblIntro As Label
    Friend WithEvents lblOld As Label
    Friend WithEvents txtOld As TextBox
    Friend WithEvents lblNew As Label
    Friend WithEvents txtNew As TextBox
    Friend WithEvents lblConfirm As Label
    Friend WithEvents txtConfirm As TextBox
    Friend WithEvents lblHint As Label
    Friend WithEvents lblMessage As Label
    Friend WithEvents pnlFooter As Panel
    Friend WithEvents btnSave As Button
    Friend WithEvents btnCancel As Button

End Class
