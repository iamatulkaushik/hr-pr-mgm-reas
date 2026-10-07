Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

Public Class frmMonthlyAttendance
    Inherits BaseForm

    Private Const FORM_WIDTH As Integer = 1100
    Private Const FORM_HEIGHT As Integer = 600
    Private Const COL_CODE As String = "colCode"
    Private Const COL_NAME As String = "colName"
    Private Const COL_WORKED As String = "colWorked"
    Private Const COL_WO As String = "colWo"
    Private Const COL_HOL As String = "colHol"
    Private Const COL_CL As String = "colCl"
    Private Const COL_EL As String = "colEl"
    Private Const COL_SL As String = "colSl"
    Private Const COL_CO As String = "colCo"
    Private Const COL_OT As String = "colOt"
    Private Const COL_ABSENT As String = "colAbsent"
    Private Const COL_PAID As String = "colPaid"
    Private Const DAYS_FORMAT As String = "0.0"
    Private Const OT_FORMAT As String = "0.00"

    Private ReadOnly _service As AttendanceService
    Private ReadOnly _canEdit As Boolean
    Private ReadOnly _dtpMonth As New DateTimePicker()
    Private ReadOnly _txtFind As New TextBox()
    Private ReadOnly _grid As New DataGridView()
    Private ReadOnly _lblInfo As New Label()
    Private ReadOnly _lblStatus As New Label()
    Private ReadOnly _btnLoad As Button = UiKit.CreateButton("Load (F5)", ButtonKind.Normal)
    Private ReadOnly _btnFromDaily As Button = UiKit.CreateButton("From daily", ButtonKind.Normal)
    Private ReadOnly _btnSave As Button = UiKit.CreateButton("Save", ButtonKind.Primary)
    Private ReadOnly _btnClose As Button = UiKit.CreateButton("Close", ButtonKind.Normal)
    Private _rows As New List(Of MonthlyAttendanceRow)()
    Private _loadedYear As Integer
    Private _loadedMonth As Integer
    Private _monthClosed As Boolean
    Private _dirty As Boolean
    Private _loading As Boolean

    Public Sub New(service As AttendanceService, user As IUserContext)
        MyBase.New()
        _service = service
        _canEdit = user.CanAccess(AccessModules.ATTENDANCE, AccessActions.EDIT)
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        Text = "Monthly Attendance"
        ClientSize = New Size(FORM_WIDTH, FORM_HEIGHT)

        UiKit.StyleGrid(_grid, False)
        _grid.Dock = DockStyle.Fill
        UiKit.AddTextColumn(_grid, COL_CODE, "Code", 8.0F, True, False)
        UiKit.AddTextColumn(_grid, COL_NAME, "Employee", 20.0F, True, False)
        UiKit.AddTextColumn(_grid, COL_WORKED, "Worked", 8.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_WO, "Weekly off", 8.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_HOL, "Holidays", 8.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_CL, "CL", 6.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_EL, "EL", 6.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_SL, "SL", 6.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_CO, "Comp off", 7.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_OT, "OT hrs", 7.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_ABSENT, "Absent", 7.0F, True, True)
        UiKit.AddTextColumn(_grid, COL_PAID, "Days paid", 8.0F, True, True)

        Dim topBar As New Panel() With {.Dock = DockStyle.Top, .Height = UiKit.BAR_HEIGHT * 2}
        topBar.Controls.Add(New Label() With {.Text = "Month:", .AutoSize = True, .Left = 8, .Top = 14})
        _dtpMonth.Format = DateTimePickerFormat.Custom
        _dtpMonth.CustomFormat = UiKit.MONTH_FORMAT
        _dtpMonth.ShowUpDown = True
        _dtpMonth.MaxDate = Date.Today
        _dtpMonth.SetBounds(60, 10, 90, 24)
        topBar.Controls.Add(_dtpMonth)
        topBar.Controls.Add(New Label() With {.Text = "Find (F3):", .AutoSize = True, .Left = 170, .Top = 14})
        _txtFind.SetBounds(240, 10, 150, 24)
        topBar.Controls.Add(_txtFind)
        _btnLoad.SetBounds(410, 8, UiKit.BUTTON_WIDTH, UiKit.BUTTON_HEIGHT)
        _btnFromDaily.SetBounds(516, 8, UiKit.BUTTON_WIDTH, UiKit.BUTTON_HEIGHT)
        topBar.Controls.Add(_btnLoad)
        topBar.Controls.Add(_btnFromDaily)
        _lblInfo.AutoSize = True
        _lblInfo.Left = 8
        _lblInfo.Top = 50
        topBar.Controls.Add(_lblInfo)

        Dim bottomBar As New Panel() With {.Dock = DockStyle.Bottom, .Height = UiKit.FOOTER_HEIGHT}
        Dim buttons As New FlowLayoutPanel() With {
            .Dock = DockStyle.Right, .AutoSize = True, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)
        }
        buttons.Controls.Add(_btnClose)
        buttons.Controls.Add(_btnSave)
        _lblStatus.Dock = DockStyle.Fill
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft
        _lblStatus.Padding = New Padding(8, 0, 0, 0)
        bottomBar.Controls.Add(_lblStatus)
        bottomBar.Controls.Add(buttons)

        Controls.Add(_grid)
        Controls.Add(topBar)
        Controls.Add(bottomBar)

        AddHandler _dtpMonth.ValueChanged, AddressOf OnMonthChanged
        AddHandler _btnLoad.Click, AddressOf OnLoadClick
        AddHandler _btnFromDaily.Click, AddressOf OnFromDailyClick
        AddHandler _btnSave.Click, AddressOf OnSaveClick
        AddHandler _btnClose.Click, AddressOf OnCloseClick
        AddHandler _txtFind.KeyDown, AddressOf OnFindKeyDown
        AddHandler _grid.CellValidating, AddressOf OnCellValidating
        AddHandler _grid.CellEndEdit, AddressOf OnCellEndEdit
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        LoadMonth()
    End Sub

    Protected Overrides Sub OnFormClosing(e As FormClosingEventArgs)
        If _dirty AndAlso e.CloseReason = CloseReason.UserClosing Then
            If Not UiKit.Confirm(Me, "Discard the unsaved attendance changes?") Then
                e.Cancel = True
                Return
            End If
        End If
        MyBase.OnFormClosing(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.F3
                _txtFind.Focus()
                Return True
            Case Keys.F5
                ReloadWithConfirm()
                Return True
            Case Keys.Control Or Keys.S
                SaveMonth()
                Return True
            Case Keys.Escape
                If _grid.IsCurrentCellInEditMode Then
                    Return MyBase.ProcessCmdKey(msg, keyData)
                End If
                Close()
                Return True
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub LoadMonth()
        _loading = True
        Try
            Dim year As Integer = _dtpMonth.Value.Year
            Dim month As Integer = _dtpMonth.Value.Month
            _rows = _service.GetMonthly(year, month)
            _monthClosed = _service.IsMonthClosed(year, month)
            _loadedYear = year
            _loadedMonth = month
            FillGrid()
            _dirty = False
            UpdateInfo()
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        Finally
            _loading = False
        End Try
    End Sub

    Private Sub FillGrid()
        _grid.Rows.Clear()
        Dim locked As Boolean = _monthClosed OrElse Not _canEdit
        For Each r As MonthlyAttendanceRow In _rows
            Dim index As Integer = _grid.Rows.Add(r.EmpCode, r.EmployeeName)
            Dim gridRow As DataGridViewRow = _grid.Rows(index)
            gridRow.Tag = r
            ShowValues(gridRow, r)
            If r.IsLocked OrElse locked Then
                gridRow.ReadOnly = True
                gridRow.DefaultCellStyle.BackColor = UiKit.LockedBack
            End If
        Next
        _btnSave.Enabled = Not locked
        _btnFromDaily.Enabled = Not locked
    End Sub

    Private Sub ShowValues(gridRow As DataGridViewRow, r As MonthlyAttendanceRow)
        gridRow.Cells(COL_WORKED).Value = r.DaysWorked.ToString(DAYS_FORMAT)
        gridRow.Cells(COL_WO).Value = r.WeeklyOff.ToString(DAYS_FORMAT)
        gridRow.Cells(COL_HOL).Value = r.Holidays.ToString(DAYS_FORMAT)
        gridRow.Cells(COL_CL).Value = r.CasualLeave.ToString(DAYS_FORMAT)
        gridRow.Cells(COL_EL).Value = r.EarnedLeave.ToString(DAYS_FORMAT)
        gridRow.Cells(COL_SL).Value = r.SickLeave.ToString(DAYS_FORMAT)
        gridRow.Cells(COL_CO).Value = r.CompLeave.ToString(DAYS_FORMAT)
        gridRow.Cells(COL_OT).Value = r.OtHours.ToString(OT_FORMAT)
        gridRow.Cells(COL_ABSENT).Value = r.Absent.ToString(DAYS_FORMAT)
        gridRow.Cells(COL_PAID).Value = r.DaysPaid.ToString(DAYS_FORMAT)
    End Sub

    Private Sub UpdateInfo()
        Dim days As Integer = AttendanceEngine.DaysInMonth(_loadedYear, _loadedMonth)
        If _monthClosed Then
            _lblInfo.ForeColor = UiKit.Danger
            _lblInfo.Text = $"Month {_loadedMonth:00}-{_loadedYear} is closed. Attendance is read-only."
        ElseIf Not _canEdit Then
            _lblInfo.ForeColor = UiKit.Danger
            _lblInfo.Text = "View only. You do not have permission to edit attendance."
        Else
            _lblInfo.ForeColor = UiKit.Primary
            _lblInfo.Text = $"{_rows.Count} employees. {days} days in the month. Values are in steps of 0.5. Absent and Days paid are calculated."
        End If
    End Sub

    Private Sub ReloadWithConfirm()
        If _dirty AndAlso Not UiKit.Confirm(Me, "Discard the unsaved attendance changes and reload?") Then
            Return
        End If
        LoadMonth()
    End Sub

    Private Sub SaveMonth()
        If Not _canEdit OrElse _monthClosed Then
            Return
        End If
        If Not _grid.EndEdit() Then
            Return
        End If
        Try
            Dim saved As Integer = _service.SaveMonthly(_loadedYear, _loadedMonth, _rows)
            _dirty = False
            UiKit.ShowSuccess(Me, $"Monthly attendance saved ({saved} rows).", _lblStatus)
            LoadMonth()
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Function TryParseDays(text As String, ByRef value As Decimal) As Boolean
        If text.Length = 0 Then
            value = 0D
            Return True
        End If
        Return Decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, value)
    End Function

    Private Sub OnCellValidating(sender As Object, e As DataGridViewCellValidatingEventArgs)
        If _loading OrElse e.RowIndex < 0 Then
            Return
        End If
        Dim name As String = _grid.Columns(e.ColumnIndex).Name
        If name = COL_CODE OrElse name = COL_NAME OrElse name = COL_ABSENT OrElse name = COL_PAID Then
            Return
        End If
        Dim text As String = If(e.FormattedValue, String.Empty).ToString().Trim()
        Dim value As Decimal
        Dim problem As String = String.Empty
        Dim days As Integer = AttendanceEngine.DaysInMonth(_loadedYear, _loadedMonth)
        If Not TryParseDays(text, value) Then
            problem = "Enter a number."
        ElseIf value < 0D Then
            problem = "The value cannot be negative."
        ElseIf name <> COL_OT AndAlso Not AttendanceEngine.IsHalfStep(value) Then
            problem = "Enter days in steps of 0.5."
        ElseIf name <> COL_OT AndAlso value > days Then
            problem = $"The value cannot exceed {days} days."
        End If
        If problem.Length > 0 Then
            e.Cancel = True
            UiKit.ShowProblem(_lblStatus, problem)
        Else
            _lblStatus.Text = String.Empty
        End If
    End Sub

    Private Sub OnCellEndEdit(sender As Object, e As DataGridViewCellEventArgs)
        If _loading OrElse e.RowIndex < 0 Then
            Return
        End If
        Dim gridRow As DataGridViewRow = _grid.Rows(e.RowIndex)
        Dim item As MonthlyAttendanceRow = DirectCast(gridRow.Tag, MonthlyAttendanceRow)
        item.DaysWorked = ReadValue(gridRow, COL_WORKED)
        item.WeeklyOff = ReadValue(gridRow, COL_WO)
        item.Holidays = ReadValue(gridRow, COL_HOL)
        item.CasualLeave = ReadValue(gridRow, COL_CL)
        item.EarnedLeave = ReadValue(gridRow, COL_EL)
        item.SickLeave = ReadValue(gridRow, COL_SL)
        item.CompLeave = ReadValue(gridRow, COL_CO)
        item.OtHours = ReadValue(gridRow, COL_OT)
        AttendanceEngine.ApplyDerived(item, AttendanceEngine.DaysInMonth(_loadedYear, _loadedMonth))
        ShowValues(gridRow, item)
        _dirty = True
    End Sub

    Private Function ReadValue(gridRow As DataGridViewRow, columnName As String) As Decimal
        Dim value As Decimal
        TryParseDays(Convert.ToString(gridRow.Cells(columnName).Value, CultureInfo.CurrentCulture).Trim(), value)
        Return value
    End Function

    Private Sub OnFindKeyDown(sender As Object, e As KeyEventArgs)
        If e.KeyCode <> Keys.Enter Then
            Return
        End If
        e.SuppressKeyPress = True
        Dim term As String = _txtFind.Text.Trim()
        If term.Length = 0 OrElse _grid.Rows.Count = 0 Then
            Return
        End If
        Dim start As Integer = If(_grid.CurrentRow Is Nothing, 0, _grid.CurrentRow.Index + 1)
        For i As Integer = 0 To _grid.Rows.Count - 1
            Dim index As Integer = (start + i) Mod _grid.Rows.Count
            Dim item As MonthlyAttendanceRow = DirectCast(_grid.Rows(index).Tag, MonthlyAttendanceRow)
            If item.EmpCode.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               item.EmployeeName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 Then
                _grid.CurrentCell = _grid.Rows(index).Cells(COL_WORKED)
                _grid.Focus()
                Return
            End If
        Next
        UiKit.ShowProblem(_lblStatus, "No employee matches.")
    End Sub

    Private Sub OnMonthChanged(sender As Object, e As EventArgs)
        If _loading Then
            Return
        End If
        If _dirty AndAlso Not UiKit.Confirm(Me, "Discard the unsaved attendance changes and change the month?") Then
            _loading = True
            _dtpMonth.Value = New Date(_loadedYear, _loadedMonth, 1)
            _loading = False
            Return
        End If
        LoadMonth()
    End Sub

    Private Sub OnLoadClick(sender As Object, e As EventArgs)
        ReloadWithConfirm()
    End Sub

    Private Sub OnFromDailyClick(sender As Object, e As EventArgs)
        If Not UiKit.Confirm(Me, "Replace the monthly values with totals from daily attendance? " &
                                 "Employees with daily entries are overwritten. Days without a daily entry count as absent.") Then
            Return
        End If
        Try
            _rows = _service.FillMonthlyFromDaily(_loadedYear, _loadedMonth)
            _loading = True
            FillGrid()
            _loading = False
            _dirty = True
            UiKit.ShowSuccess(Me, "Filled from daily attendance. Review and save.", _lblStatus)
        Catch ex As BusinessRuleException
            _loading = False
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub OnSaveClick(sender As Object, e As EventArgs)
        SaveMonth()
    End Sub

    Private Sub OnCloseClick(sender As Object, e As EventArgs)
        Close()
    End Sub

End Class
