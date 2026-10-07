Imports System.Drawing
Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

Public Class frmMonthClose
    Inherits BaseForm

    Private Const FORM_WIDTH As Integer = 900
    Private Const FORM_HEIGHT As Integer = 560
    Private Const LOCAL_TIME_FORMAT As String = "dd-MM-yyyy HH:mm"

    Private ReadOnly _service As MonthCloseService
    Private ReadOnly _canClose As Boolean
    Private ReadOnly _canReopen As Boolean
    Private ReadOnly _dtpMonth As New DateTimePicker()
    Private ReadOnly _lblState As New Label()
    Private ReadOnly _lblDetail As New Label()
    Private ReadOnly _lblPreview As New Label()
    Private ReadOnly _history As New ListView()
    Private ReadOnly _lblStatus As New Label()
    Private ReadOnly _btnRefresh As Button = UiKit.CreateButton("Refresh (F5)", ButtonKind.Normal)
    Private ReadOnly _btnCloseMonth As Button = UiKit.CreateButton("Close month", ButtonKind.Primary)
    Private ReadOnly _btnReopen As Button = UiKit.CreateButton("Reopen...", ButtonKind.Danger)
    Private ReadOnly _btnExit As Button = UiKit.CreateButton("Exit", ButtonKind.Normal)
    Private _isClosed As Boolean

    Public Sub New(service As MonthCloseService, user As IUserContext)
        MyBase.New()
        _service = service
        _canClose = user.CanAccess(AccessModules.ATTENDANCE, AccessActions.MONTH_CLOSE)
        _canReopen = user.CanAccess(AccessModules.ATTENDANCE, AccessActions.REOPEN)
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        Text = "Month Close"
        ClientSize = New Size(FORM_WIDTH, FORM_HEIGHT)

        Dim topBar As New Panel() With {.Dock = DockStyle.Top, .Height = UiKit.BAR_HEIGHT}
        topBar.Controls.Add(New Label() With {.Text = "Month:", .AutoSize = True, .Left = 8, .Top = 14})
        _dtpMonth.Format = DateTimePickerFormat.Custom
        _dtpMonth.CustomFormat = UiKit.MONTH_FORMAT
        _dtpMonth.ShowUpDown = True
        _dtpMonth.MaxDate = Date.Today
        _dtpMonth.SetBounds(60, 10, 90, 24)
        _btnRefresh.SetBounds(170, 8, UiKit.BUTTON_WIDTH, UiKit.BUTTON_HEIGHT)
        topBar.Controls.Add(_dtpMonth)
        topBar.Controls.Add(_btnRefresh)

        Dim tabs As New TabControl() With {.Dock = DockStyle.Fill}
        Dim page As New TabPage("Status")
        _lblState.AutoSize = True
        _lblState.Font = New Font(Font.FontFamily, 20.0F, FontStyle.Bold)
        _lblState.Location = New Point(16, 16)
        _lblDetail.AutoSize = True
        _lblDetail.Location = New Point(18, 62)
        _lblPreview.AutoSize = True
        _lblPreview.Location = New Point(18, 88)
        Dim historyTitle As New Label() With {.Text = "History", .AutoSize = True, .Location = New Point(18, 124)}
        _history.View = View.Details
        _history.FullRowSelect = True
        _history.GridLines = True
        _history.Columns.Add("Action", 90)
        _history.Columns.Add("When", 140)
        _history.Columns.Add("User", 120)
        _history.Columns.Add("Reason", 460)
        _history.SetBounds(18, 146, FORM_WIDTH - 60, 260)
        page.Controls.Add(_lblState)
        page.Controls.Add(_lblDetail)
        page.Controls.Add(_lblPreview)
        page.Controls.Add(historyTitle)
        page.Controls.Add(_history)
        tabs.TabPages.Add(page)

        Dim bottomBar As New Panel() With {.Dock = DockStyle.Bottom, .Height = UiKit.FOOTER_HEIGHT}
        Dim buttons As New FlowLayoutPanel() With {
            .Dock = DockStyle.Right, .AutoSize = True, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)
        }
        buttons.Controls.Add(_btnExit)
        buttons.Controls.Add(_btnReopen)
        buttons.Controls.Add(_btnCloseMonth)
        _lblStatus.Dock = DockStyle.Fill
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft
        _lblStatus.Padding = New Padding(8, 0, 0, 0)
        bottomBar.Controls.Add(_lblStatus)
        bottomBar.Controls.Add(buttons)

        Controls.Add(tabs)
        Controls.Add(topBar)
        Controls.Add(bottomBar)

        AddHandler _dtpMonth.ValueChanged, AddressOf OnMonthChanged
        AddHandler _btnRefresh.Click, AddressOf OnRefreshClick
        AddHandler _btnCloseMonth.Click, AddressOf OnCloseMonthClick
        AddHandler _btnReopen.Click, AddressOf OnReopenClick
        AddHandler _btnExit.Click, AddressOf OnExitClick
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        RefreshView()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.F5
                RefreshView()
                Return True
            Case Keys.Escape
                Close()
                Return True
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Function SelectedYear() As Integer
        Return _dtpMonth.Value.Year
    End Function

    Private Function SelectedMonth() As Integer
        Return _dtpMonth.Value.Month
    End Function

    Private Sub RefreshView()
        Try
            Dim status As MonthCloseStatus = _service.GetStatus(SelectedYear(), SelectedMonth())
            _isClosed = status.IsClosed
            If _isClosed Then
                _lblState.ForeColor = UiKit.Danger
                _lblState.Text = "CLOSED"
                Dim closedOn As String = If(status.ClosedOn.HasValue, status.ClosedOn.Value.ToLocalTime().ToString(LOCAL_TIME_FORMAT), String.Empty)
                _lblDetail.Text = $"Closed by {status.ClosedByName} on {closedOn}. Attendance is locked."
                _lblPreview.Text = String.Empty
            Else
                _lblState.ForeColor = UiKit.Accent
                _lblState.Text = "OPEN"
                _lblDetail.Text = "Operators can still add and edit attendance for this month."
                Dim missing As Integer = _service.CountMissingMonthly(SelectedYear(), SelectedMonth())
                _lblPreview.ForeColor = If(missing > 0, Color.DarkOrange, UiKit.Accent)
                _lblPreview.Text = If(missing > 0, $"{missing} employees have no monthly attendance saved.", "Every employee on the roll has monthly attendance.")
            End If
            FillHistory(_service.GetLog(SelectedYear(), SelectedMonth()))
            _btnCloseMonth.Enabled = _canClose AndAlso Not _isClosed
            _btnReopen.Enabled = _canReopen AndAlso _isClosed
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub FillHistory(entries As List(Of MonthCloseLogEntry))
        _history.Items.Clear()
        For Each entry As MonthCloseLogEntry In entries
            Dim item As New ListViewItem(entry.Action)
            item.SubItems.Add(entry.LoggedOn.ToLocalTime().ToString(LOCAL_TIME_FORMAT))
            item.SubItems.Add(entry.UserName)
            item.SubItems.Add(entry.Reason)
            _history.Items.Add(item)
        Next
    End Sub

    Private Sub OnMonthChanged(sender As Object, e As EventArgs)
        RefreshView()
    End Sub

    Private Sub OnRefreshClick(sender As Object, e As EventArgs)
        RefreshView()
    End Sub

    Private Sub OnCloseMonthClick(sender As Object, e As EventArgs)
        Dim year As Integer = SelectedYear()
        Dim month As Integer = SelectedMonth()
        Try
            Dim missing As Integer = _service.CountMissingMonthly(year, month)
            If missing > 0 AndAlso Not UiKit.Confirm(Me, $"{missing} employees have no monthly attendance saved. They will be locked with no attendance. Continue?") Then
                Return
            End If
            If Not MonthCloseService.MonthHasEnded(year, month) AndAlso Not UiKit.Confirm(Me, "This month has not ended yet. Close it anyway?") Then
                Return
            End If
            If Not UiKit.Confirm(Me, $"Close attendance for {month:00}-{year}? Nobody can edit it until a Company admin or Associate reopens it.") Then
                Return
            End If
            _service.CloseMonth(year, month)
            UiKit.ShowSuccess(Me, $"Attendance for {month:00}-{year} is closed.", _lblStatus)
            RefreshView()
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub OnReopenClick(sender As Object, e As EventArgs)
        Dim year As Integer = SelectedYear()
        Dim month As Integer = SelectedMonth()
        Using dialog As New frmReopenReason($"{month:00}-{year}")
            If dialog.ShowDialog(Me) <> DialogResult.OK Then
                Return
            End If
            Try
                _service.ReopenMonth(year, month, dialog.Reason)
                UiKit.ShowSuccess(Me, $"Attendance for {month:00}-{year} is reopened.", _lblStatus)
                RefreshView()
            Catch ex As BusinessRuleException
                UiKit.ShowWarning(Me, ex.Message)
            End Try
        End Using
    End Sub

    Private Sub OnExitClick(sender As Object, e As EventArgs)
        Close()
    End Sub

End Class
