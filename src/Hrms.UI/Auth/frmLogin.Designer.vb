Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmLogin
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
        Me.pnlHeader = New Panel()
        Me.lblAppName = New Label()
        Me.lblTagline = New Label()
        Me.lblUserName = New Label()
        Me.txtUserName = New TextBox()
        Me.lblPassword = New Label()
        Me.txtPassword = New TextBox()
        Me.lblCaps = New Label()
        Me.lblMessage = New Label()
        Me.pnlFooter = New Panel()
        Me.lblVersion = New Label()
        Me.btnLogin = New Button()
        Me.btnExit = New Button()
        Me.pnlHeader.SuspendLayout()
        Me.pnlFooter.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlHeader
        '
        Me.pnlHeader.BackColor = Color.FromArgb(31, 78, 121)
        Me.pnlHeader.Controls.Add(Me.lblAppName)
        Me.pnlHeader.Controls.Add(Me.lblTagline)
        Me.pnlHeader.Dock = DockStyle.Top
        Me.pnlHeader.Name = "pnlHeader"
        Me.pnlHeader.Size = New Size(420, 72)
        Me.pnlHeader.TabStop = False
        '
        'lblAppName
        '
        Me.lblAppName.AutoSize = True
        Me.lblAppName.Font = New Font("Segoe UI", 15.0F, FontStyle.Bold)
        Me.lblAppName.ForeColor = Color.White
        Me.lblAppName.Location = New Point(20, 10)
        Me.lblAppName.Name = "lblAppName"
        Me.lblAppName.Text = "Revolution HRMS"
        '
        'lblTagline
        '
        Me.lblTagline.AutoSize = True
        Me.lblTagline.ForeColor = Color.FromArgb(220, 230, 240)
        Me.lblTagline.Location = New Point(22, 44)
        Me.lblTagline.Name = "lblTagline"
        Me.lblTagline.Text = "Offline HR and Payroll - Haryana"
        '
        'lblUserName
        '
        Me.lblUserName.AutoSize = True
        Me.lblUserName.Location = New Point(40, 96)
        Me.lblUserName.Name = "lblUserName"
        Me.lblUserName.Text = "User name"
        '
        'txtUserName
        '
        Me.txtUserName.Location = New Point(40, 118)
        Me.txtUserName.MaxLength = 50
        Me.txtUserName.Name = "txtUserName"
        Me.txtUserName.Size = New Size(340, 25)
        Me.txtUserName.TabIndex = 0
        '
        'lblPassword
        '
        Me.lblPassword.AutoSize = True
        Me.lblPassword.Location = New Point(40, 158)
        Me.lblPassword.Name = "lblPassword"
        Me.lblPassword.Text = "Password"
        '
        'txtPassword
        '
        Me.txtPassword.Location = New Point(40, 180)
        Me.txtPassword.MaxLength = 64
        Me.txtPassword.Name = "txtPassword"
        Me.txtPassword.Size = New Size(340, 25)
        Me.txtPassword.TabIndex = 1
        Me.txtPassword.UseSystemPasswordChar = True
        '
        'lblCaps
        '
        Me.lblCaps.AutoSize = True
        Me.lblCaps.ForeColor = Color.FromArgb(179, 107, 0)
        Me.lblCaps.Location = New Point(40, 210)
        Me.lblCaps.Name = "lblCaps"
        Me.lblCaps.Text = "Caps Lock is on"
        Me.lblCaps.Visible = False
        '
        'lblMessage
        '
        Me.lblMessage.ForeColor = Color.FromArgb(178, 34, 34)
        Me.lblMessage.Location = New Point(40, 236)
        Me.lblMessage.Name = "lblMessage"
        Me.lblMessage.Size = New Size(340, 44)
        '
        'pnlFooter
        '
        Me.pnlFooter.Controls.Add(Me.lblVersion)
        Me.pnlFooter.Controls.Add(Me.btnLogin)
        Me.pnlFooter.Controls.Add(Me.btnExit)
        Me.pnlFooter.Dock = DockStyle.Bottom
        Me.pnlFooter.Name = "pnlFooter"
        Me.pnlFooter.Size = New Size(420, 64)
        Me.pnlFooter.TabStop = False
        '
        'lblVersion
        '
        Me.lblVersion.AutoSize = True
        Me.lblVersion.ForeColor = Color.DimGray
        Me.lblVersion.Location = New Point(16, 24)
        Me.lblVersion.Name = "lblVersion"
        '
        'btnLogin
        '
        Me.btnLogin.Location = New Point(196, 15)
        Me.btnLogin.Name = "btnLogin"
        Me.btnLogin.Size = New Size(100, 34)
        Me.btnLogin.TabIndex = 2
        Me.btnLogin.Tag = "primary"
        Me.btnLogin.Text = "Sign in"
        '
        'btnExit
        '
        Me.btnExit.DialogResult = DialogResult.Cancel
        Me.btnExit.Location = New Point(304, 15)
        Me.btnExit.Name = "btnExit"
        Me.btnExit.Size = New Size(76, 34)
        Me.btnExit.TabIndex = 3
        Me.btnExit.Text = "Exit"
        '
        'frmLogin
        '
        Me.AcceptButton = Me.btnLogin
        Me.AutoScaleDimensions = New SizeF(7.0F, 17.0F)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.CancelButton = Me.btnExit
        Me.ClientSize = New Size(420, 360)
        Me.Controls.Add(Me.lblMessage)
        Me.Controls.Add(Me.lblCaps)
        Me.Controls.Add(Me.txtPassword)
        Me.Controls.Add(Me.lblPassword)
        Me.Controls.Add(Me.txtUserName)
        Me.Controls.Add(Me.lblUserName)
        Me.Controls.Add(Me.pnlFooter)
        Me.Controls.Add(Me.pnlHeader)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmLogin"
        Me.StartPosition = FormStartPosition.CenterScreen
        Me.Text = "Revolution HRMS - Sign in"
        Me.pnlHeader.ResumeLayout(False)
        Me.pnlHeader.PerformLayout()
        Me.pnlFooter.ResumeLayout(False)
        Me.pnlFooter.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()
    End Sub

    Friend WithEvents pnlHeader As Panel
    Friend WithEvents lblAppName As Label
    Friend WithEvents lblTagline As Label
    Friend WithEvents lblUserName As Label
    Friend WithEvents txtUserName As TextBox
    Friend WithEvents lblPassword As Label
    Friend WithEvents txtPassword As TextBox
    Friend WithEvents lblCaps As Label
    Friend WithEvents lblMessage As Label
    Friend WithEvents pnlFooter As Panel
    Friend WithEvents lblVersion As Label
    Friend WithEvents btnLogin As Button
    Friend WithEvents btnExit As Button

    Public Sub New()
        InitializeComponent()
    End Sub

End Class
