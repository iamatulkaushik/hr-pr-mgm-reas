Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmCompanySelect
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
        Me.pnlTop = New Panel()
        Me.lblSearch = New Label()
        Me.txtSearch = New TextBox()
        Me.dgvCompanies = New DataGridView()
        Me.tabDetails = New TabControl()
        Me.tpgDetails = New TabPage()
        Me.lblNameCap = New Label()
        Me.lblNameVal = New Label()
        Me.lblAddressCap = New Label()
        Me.lblAddressVal = New Label()
        Me.lblPanCap = New Label()
        Me.lblPanVal = New Label()
        Me.lblTanCap = New Label()
        Me.lblTanVal = New Label()
        Me.lblEpfCap = New Label()
        Me.lblEpfVal = New Label()
        Me.lblEsiCap = New Label()
        Me.lblEsiVal = New Label()
        Me.pnlBottom = New Panel()
        Me.btnSelect = New Button()
        Me.btnCancel = New Button()
        Me.pnlTop.SuspendLayout()
        CType(Me.dgvCompanies, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.tabDetails.SuspendLayout()
        Me.tpgDetails.SuspendLayout()
        Me.pnlBottom.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlTop
        '
        Me.pnlTop.Controls.Add(Me.lblSearch)
        Me.pnlTop.Controls.Add(Me.txtSearch)
        Me.pnlTop.Dock = DockStyle.Top
        Me.pnlTop.Name = "pnlTop"
        Me.pnlTop.Size = New Size(780, 48)
        Me.pnlTop.TabStop = False
        '
        'lblSearch
        '
        Me.lblSearch.AutoSize = True
        Me.lblSearch.Location = New Point(12, 14)
        Me.lblSearch.Name = "lblSearch"
        Me.lblSearch.Text = "Find (F3)"
        '
        'txtSearch
        '
        Me.txtSearch.Location = New Point(88, 11)
        Me.txtSearch.Name = "txtSearch"
        Me.txtSearch.Size = New Size(280, 25)
        Me.txtSearch.TabIndex = 0
        '
        'dgvCompanies
        '
        Me.dgvCompanies.AllowUserToAddRows = False
        Me.dgvCompanies.AllowUserToDeleteRows = False
        Me.dgvCompanies.AllowUserToResizeRows = False
        Me.dgvCompanies.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dgvCompanies.Dock = DockStyle.Left
        Me.dgvCompanies.MultiSelect = False
        Me.dgvCompanies.Name = "dgvCompanies"
        Me.dgvCompanies.ReadOnly = True
        Me.dgvCompanies.RowHeadersVisible = False
        Me.dgvCompanies.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        Me.dgvCompanies.Size = New Size(360, 340)
        Me.dgvCompanies.TabIndex = 1
        '
        'tabDetails
        '
        Me.tabDetails.Controls.Add(Me.tpgDetails)
        Me.tabDetails.Dock = DockStyle.Fill
        Me.tabDetails.Name = "tabDetails"
        Me.tabDetails.TabStop = False
        '
        'tpgDetails
        '
        Me.tpgDetails.BackColor = Color.White
        Me.tpgDetails.Controls.Add(Me.lblNameCap)
        Me.tpgDetails.Controls.Add(Me.lblNameVal)
        Me.tpgDetails.Controls.Add(Me.lblAddressCap)
        Me.tpgDetails.Controls.Add(Me.lblAddressVal)
        Me.tpgDetails.Controls.Add(Me.lblPanCap)
        Me.tpgDetails.Controls.Add(Me.lblPanVal)
        Me.tpgDetails.Controls.Add(Me.lblTanCap)
        Me.tpgDetails.Controls.Add(Me.lblTanVal)
        Me.tpgDetails.Controls.Add(Me.lblEpfCap)
        Me.tpgDetails.Controls.Add(Me.lblEpfVal)
        Me.tpgDetails.Controls.Add(Me.lblEsiCap)
        Me.tpgDetails.Controls.Add(Me.lblEsiVal)
        Me.tpgDetails.Name = "tpgDetails"
        Me.tpgDetails.Text = "Details"
        '
        'Caption and value labels (rows 12, 52, 112, 140, 168, 196)
        '
        Me.lblNameCap.AutoSize = True : Me.lblNameCap.ForeColor = Color.DimGray
        Me.lblNameCap.Location = New Point(12, 12) : Me.lblNameCap.Name = "lblNameCap" : Me.lblNameCap.Text = "Company"
        Me.lblNameVal.Font = New Font("Segoe UI", 10.5F, FontStyle.Bold)
        Me.lblNameVal.Location = New Point(110, 10) : Me.lblNameVal.Name = "lblNameVal" : Me.lblNameVal.Size = New Size(280, 36)
        Me.lblAddressCap.AutoSize = True : Me.lblAddressCap.ForeColor = Color.DimGray
        Me.lblAddressCap.Location = New Point(12, 52) : Me.lblAddressCap.Name = "lblAddressCap" : Me.lblAddressCap.Text = "Address"
        Me.lblAddressVal.Location = New Point(110, 52) : Me.lblAddressVal.Name = "lblAddressVal" : Me.lblAddressVal.Size = New Size(280, 52)
        Me.lblPanCap.AutoSize = True : Me.lblPanCap.ForeColor = Color.DimGray
        Me.lblPanCap.Location = New Point(12, 112) : Me.lblPanCap.Name = "lblPanCap" : Me.lblPanCap.Text = "PAN"
        Me.lblPanVal.Location = New Point(110, 112) : Me.lblPanVal.Name = "lblPanVal" : Me.lblPanVal.Size = New Size(280, 22)
        Me.lblTanCap.AutoSize = True : Me.lblTanCap.ForeColor = Color.DimGray
        Me.lblTanCap.Location = New Point(12, 140) : Me.lblTanCap.Name = "lblTanCap" : Me.lblTanCap.Text = "TAN"
        Me.lblTanVal.Location = New Point(110, 140) : Me.lblTanVal.Name = "lblTanVal" : Me.lblTanVal.Size = New Size(280, 22)
        Me.lblEpfCap.AutoSize = True : Me.lblEpfCap.ForeColor = Color.DimGray
        Me.lblEpfCap.Location = New Point(12, 168) : Me.lblEpfCap.Name = "lblEpfCap" : Me.lblEpfCap.Text = "EPF code"
        Me.lblEpfVal.Location = New Point(110, 168) : Me.lblEpfVal.Name = "lblEpfVal" : Me.lblEpfVal.Size = New Size(280, 22)
        Me.lblEsiCap.AutoSize = True : Me.lblEsiCap.ForeColor = Color.DimGray
        Me.lblEsiCap.Location = New Point(12, 196) : Me.lblEsiCap.Name = "lblEsiCap" : Me.lblEsiCap.Text = "ESI code"
        Me.lblEsiVal.Location = New Point(110, 196) : Me.lblEsiVal.Name = "lblEsiVal" : Me.lblEsiVal.Size = New Size(280, 22)
        '
        'pnlBottom
        '
        Me.pnlBottom.Controls.Add(Me.btnSelect)
        Me.pnlBottom.Controls.Add(Me.btnCancel)
        Me.pnlBottom.Dock = DockStyle.Bottom
        Me.pnlBottom.Name = "pnlBottom"
        Me.pnlBottom.Size = New Size(780, 56)
        Me.pnlBottom.TabStop = False
        '
        'btnSelect
        '
        Me.btnSelect.Location = New Point(560, 11)
        Me.btnSelect.Name = "btnSelect"
        Me.btnSelect.Size = New Size(100, 34)
        Me.btnSelect.TabIndex = 2
        Me.btnSelect.Tag = "primary"
        Me.btnSelect.Text = "Select"
        '
        'btnCancel
        '
        Me.btnCancel.DialogResult = DialogResult.Cancel
        Me.btnCancel.Location = New Point(668, 11)
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.Size = New Size(100, 34)
        Me.btnCancel.TabIndex = 3
        Me.btnCancel.Text = "Cancel"
        '
        'frmCompanySelect
        '
        Me.AcceptButton = Me.btnSelect
        Me.AutoScaleDimensions = New SizeF(7.0F, 17.0F)
        Me.AutoScaleMode = AutoScaleMode.Font
        Me.CancelButton = Me.btnCancel
        Me.ClientSize = New Size(780, 444)
        Me.Controls.Add(Me.tabDetails)
        Me.Controls.Add(Me.dgvCompanies)
        Me.Controls.Add(Me.pnlBottom)
        Me.Controls.Add(Me.pnlTop)
        Me.FormBorderStyle = FormBorderStyle.FixedDialog
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmCompanySelect"
        Me.ShowInTaskbar = False
        Me.StartPosition = FormStartPosition.CenterParent
        Me.Text = "Select company"
        Me.pnlTop.ResumeLayout(False)
        Me.pnlTop.PerformLayout()
        CType(Me.dgvCompanies, System.ComponentModel.ISupportInitialize).EndInit()
        Me.tabDetails.ResumeLayout(False)
        Me.tpgDetails.ResumeLayout(False)
        Me.tpgDetails.PerformLayout()
        Me.pnlBottom.ResumeLayout(False)
        Me.ResumeLayout(False)
    End Sub

    Friend WithEvents pnlTop As Panel
    Friend WithEvents lblSearch As Label
    Friend WithEvents txtSearch As TextBox
    Friend WithEvents dgvCompanies As DataGridView
    Friend WithEvents tabDetails As TabControl
    Friend WithEvents tpgDetails As TabPage
    Friend WithEvents lblNameCap As Label
    Friend WithEvents lblNameVal As Label
    Friend WithEvents lblAddressCap As Label
    Friend WithEvents lblAddressVal As Label
    Friend WithEvents lblPanCap As Label
    Friend WithEvents lblPanVal As Label
    Friend WithEvents lblTanCap As Label
    Friend WithEvents lblTanVal As Label
    Friend WithEvents lblEpfCap As Label
    Friend WithEvents lblEpfVal As Label
    Friend WithEvents lblEsiCap As Label
    Friend WithEvents lblEsiVal As Label
    Friend WithEvents pnlBottom As Panel
    Friend WithEvents btnSelect As Button
    Friend WithEvents btnCancel As Button

End Class
