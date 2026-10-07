Imports System.Drawing
Imports System.Windows.Forms
Imports Hrms.BLL

' Modal dialog that collects the audit reason for reopening a closed month.
Public Class frmReopenReason
    Inherits Form

    Private Const FORM_WIDTH As Integer = 460
    Private Const FORM_HEIGHT As Integer = 270
    Private Const REASON_MAX_LENGTH As Integer = 500

    Private ReadOnly _txtReason As New TextBox()
    Private ReadOnly _lblCount As New Label()
    Private ReadOnly _btnOk As Button = UiKit.CreateButton("Reopen", ButtonKind.Danger)
    Private ReadOnly _btnCancel As Button = UiKit.CreateButton("Cancel", ButtonKind.Normal)

    Public ReadOnly Property Reason As String
        Get
            Return _txtReason.Text.Trim()
        End Get
    End Property

    Public Sub New(monthText As String)
        MyBase.New()
        Text = $"Reopen attendance {monthText}"
        FormBorderStyle = FormBorderStyle.FixedDialog
        StartPosition = FormStartPosition.CenterParent
        MaximizeBox = False
        MinimizeBox = False
        ShowInTaskbar = False
        ClientSize = New Size(FORM_WIDTH, FORM_HEIGHT)

        Dim prompt As Control = UiKit.RequiredLabel("Reason for reopening (saved in the audit log)")
        prompt.Location = New Point(12, 12)
        _txtReason.Multiline = True
        _txtReason.MaxLength = REASON_MAX_LENGTH
        _txtReason.ScrollBars = ScrollBars.Vertical
        _txtReason.SetBounds(12, 40, FORM_WIDTH - 24, 130)
        _lblCount.AutoSize = True
        _lblCount.Location = New Point(12, 178)
        _btnOk.Location = New Point(FORM_WIDTH - 2 * UiKit.BUTTON_WIDTH - 24, FORM_HEIGHT - UiKit.BUTTON_HEIGHT - 16)
        _btnCancel.Location = New Point(FORM_WIDTH - UiKit.BUTTON_WIDTH - 12, FORM_HEIGHT - UiKit.BUTTON_HEIGHT - 16)
        _btnOk.DialogResult = DialogResult.OK
        _btnCancel.DialogResult = DialogResult.Cancel
        AcceptButton = _btnOk
        CancelButton = _btnCancel

        Controls.Add(prompt)
        Controls.Add(_txtReason)
        Controls.Add(_lblCount)
        Controls.Add(_btnOk)
        Controls.Add(_btnCancel)

        AddHandler _txtReason.TextChanged, AddressOf OnReasonChanged
        UpdateState()
    End Sub

    Private Sub UpdateState()
        Dim length As Integer = _txtReason.Text.Trim().Length
        Dim enough As Boolean = length >= MonthCloseService.MIN_REASON_LENGTH
        _btnOk.Enabled = enough
        _lblCount.ForeColor = If(enough, UiKit.Accent, UiKit.Danger)
        _lblCount.Text = $"{length} of at least {MonthCloseService.MIN_REASON_LENGTH} characters"
    End Sub

    Private Sub OnReasonChanged(sender As Object, e As EventArgs)
        UpdateState()
    End Sub

End Class
