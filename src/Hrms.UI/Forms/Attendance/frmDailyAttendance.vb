Imports System.Drawing
Imports System.Globalization
Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

Public Class frmDailyAttendance
    Inherits BaseForm

    Private Const FORM_WIDTH As Integer = 980
    Private Const FORM_HEIGHT As Integer = 600
    Private Const COL_CODE As String = "colCode"
    Private Const COL_NAME As String = "colName"
    Private Const COL_STATUS As String = "colStatus"
    Private Const COL_IN As String = "colIn"
    Private Const COL_OUT As String = "colOut"
    Private Const COL_OT As String = "colOt"
    Private Const COL_LOCK As String = "colLock"
    Private Const OT_FORMAT As String = "0.00"
    Private Const SHIFT_MIN As Decimal = 0.5D
    Private Const SHIFT_MAX As Decimal = 24D
    Private Const LEGEND As String = "P Present   A Absent   HD Half day   H Holiday   WO Weekly off   CL/EL/SL Leave   CO Comp off"

    Private ReadOnly _service As AttendanceService
    Private ReadOnly _canEdit As Boolean
    Private ReadOnly _dtpDate As New DateTimePicker()
    Private ReadOnly _nudShift As New NumericUpDown()
    Private ReadOnly _txtFind As New TextBox()
    Private ReadOnly _grid As New DataGridView()
    Private ReadOnly _lblInfo As New Label()
    Private ReadOnly _lblStatus As New Label()
    Private ReadOnly _btnLoad As Button = UiKit.CreateButton("Load (F5)", ButtonKind.Normal)
    Private ReadOnly _btnFill As Button = UiKit.CreateButton("Fill blanks", ButtonKind.Normal)
    Private ReadOnly _btnSave As Button = UiKit.CreateButton("Save", ButtonKind.Primary)
    Private ReadOnly _btnClose As Button = UiKit.CreateButton("Close", ButtonKind.Normal)
    Private _rows As New List(Of DailyAttendanceRow)()
    Private _loadedDate As Date
    Private _dayClosed As Boolean
    Private _dirty As Boolean
    Private _loading As Boolean

    Public Sub New(service As AttendanceService, user As IUserContext)
        MyBase.New()
        _service = service
        _canEdit = user.CanAccess(AccessModules.ATTENDANCE, AccessActions.EDIT)
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        Text = "Daily Attendance"
        ClientSize = New Size(FORM_WIDTH, FORM_HEIGHT)

        UiKit.StyleGrid(_grid, False)
        _grid.Dock = DockStyle.Fill
        UiKit.AddTextColumn(_grid, COL_CODE, "Code", 12.0F, True, False)
        UiKit.AddTextColumn(_grid, COL_NAME, "Employee", 34.0F, True, False)
        UiKit.AddTextColumn(_grid, COL_STATUS, "Status", 10.0F, False, False)
        UiKit.AddTextColumn(_grid, COL_IN, "In", 11.0F, False, False)
        UiKit.AddTextColumn(_grid, COL_OUT, "Out", 11.0F, False, False)
        UiKit.AddTextColumn(_grid, COL_OT, "OT hrs", 11.0F, False, True)
        UiKit.AddTextColumn(_grid, COL_LOCK, "Lock", 11.0F, True, False)

        Dim topBar As New Panel() With {.Dock = DockStyle.Top, .Height = UiKit.BAR_HEIGHT * 2}
        topBar.Controls.Add(New Label() With {.Text = "Date:", .AutoSize = True, .Left = 8, .Top = 14})
        _dtpDate.Format = DateTimePickerFormat.Custom
        _dtpDate.CustomFormat = UiKit.DATE_FORMAT
        _dtpDate.MaxDate = Date.Today
        _dtpDate.SetBounds(50, 10, 110, 24)
        topBar.Controls.Add(_dtpDate)
        topBar.Controls.Add(New Label() With {.Text = "Shift hrs:", .AutoSize = True, .Left = 180, .Top = 14})
        _nudShift.DecimalPlaces = 1
        _nudShift.Increment = SHIFT_MIN
        _nudShift.Minimum = SHIFT_MIN
        _nudShift.Maximum = SHIFT_MAX
        _nudShift.Value = OvertimeSettings.DEFAULT_SHIFT_HOURS
        _nudShift.SetBounds(245, 10, 60, 24)
        topBar.Controls.Add(_nudShift)
        topBar.Controls.Add(New Label() With {.Text = "Find (F3):", .AutoSize = True, .Left = 325, .Top = 14})
        _txtFind.SetBounds(395, 10, 150, 24)
        topBar.Controls.Add(_txtFind)
        _btnLoad.SetBounds(560, 8, UiKit.BUTTON_WIDTH, UiKit.BUTTON_HEIGHT)
        _btnFill.SetBounds(666, 8, UiKit.BUTTON_WIDTH, UiKit.BUTTON_HEIGHT)
        topBar.Controls.Add(_btnLoad)
        topBar.Controls.Add(_btnFill)
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

        AddHandler _dtpDate.ValueChanged, AddressOf OnDateChanged
        AddHandler _btnLoad.Click, AddressOf OnLoadClick
        AddHandler _btnFill.Click, AddressOf OnFillClick
        AddHandler _btnSave.Click, AddressOf OnSaveClick
        AddHandler _btnClose.Click, AddressOf OnCloseClick
        AddHandler _txtFind.KeyDown, AddressOf OnFindKeyDown
        AddHandler _grid.CellValidating, AddressOf OnCellValidating
        AddHandler _grid.CellEndEdit, AddressOf OnCellEndEdit
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        _loadedDate = _dtpDate.Value.Date
        LoadDay()
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
                SaveDay()
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

    Private Sub LoadDay()
        _loading = True
        Try
            Dim day As Date = _dtpDate.Value.Date
            _rows = _service.GetDay(day)
            _dayClosed = _service.IsMonthClosed(day.Year, day.Month)
            _loadedDate = day
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
        Dim locked As Boolean = _dayClosed OrElse Not _canEdit
        For Each r As DailyAttendanceRow In _rows
            Dim index As Integer = _grid.Rows.Add(r.EmpCode, r.EmployeeName, r.Status, UiKit.FormatTime(r.InTime),
                                                  UiKit.FormatTime(r.OutTime), r.OtHours.ToString(OT_FORMAT),
                                                  If(r.IsLocked, "Locked", String.Empty))
            Dim gridRow As DataGridViewRow = _grid.Rows(index)
            gridRow.Tag = r
            If r.IsLocked OrElse locked Then
                gridRow.ReadOnly = True
                gridRow.DefaultCellStyle.BackColor = UiKit.LockedBack
            End If
        Next
        _btnSave.Enabled = Not locked
        _btnFill.Enabled = Not locked
    End Sub

    Private Sub UpdateInfo()
        If _dayClosed Then
            _lblInfo.ForeColor = UiKit.Danger
            _lblInfo.Text = $"Month {_loadedDate:MM-yyyy} is closed. Attendance is read-only. {_rows.Count} employees."
        ElseIf Not _canEdit Then
            _lblInfo.ForeColor = UiKit.Danger
            _lblInfo.Text = "View only. You do not have permission to edit attendance."
        Else
            _lblInfo.ForeColor = UiKit.Primary
            _lblInfo.Text = $"{_rows.Count} employees.   {LEGEND}"
        End If
    End Sub

    Private Sub MarkDirty()
        _dirty = True
    End Sub

    Private Sub ReloadWithConfirm()
        If _dirty AndAlso Not UiKit.Confirm(Me, "Discard the unsaved attendance changes and reload?") Then
            Return
        End If
        LoadDay()
    End Sub

    Private Sub SaveDay()
        If Not _canEdit OrElse _dayClosed Then
            Return
        End If
        If Not _grid.EndEdit() Then
            Return
        End If
        Try
            Dim saved As Integer = _service.SaveDay(_loadedDate, _rows)
            _dirty = False
            UiKit.ShowSuccess(Me, $"Attendance saved ({saved} rows).", _lblStatus)
            LoadDay()
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Function CellText(gridRow As DataGridViewRow, columnName As String) As String
        Return Convert.ToString(gridRow.Cells(columnName).Value, CultureInfo.CurrentCulture).Trim()
    End Function

    Private Function ParseOt(text As String, ByRef value As Decimal) As Boolean
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
        Dim text As String = If(e.FormattedValue, String.Empty).ToString().Trim()
        Dim problem As String = String.Empty
        Select Case _grid.Columns(e.ColumnIndex).Name
            Case COL_STATUS
                If text.Length > 0 AndAlso Not AttendanceCodes.IsValid(text) Then
                    problem = "Status must be P, A, HD, H, WO, CL, EL, SL or CO."
                End If
            Case COL_IN, COL_OUT
                Dim parsed As TimeSpan
                If text.Length > 0 AndAlso Not UiKit.TryParseTime(text, parsed) Then
                    problem = "Enter time as HH:mm, for example 09:30."
                End If
            Case COL_OT
                Dim hours As Decimal
                If Not ParseOt(text, hours) OrElse hours < 0D OrElse hours > AttendanceEngine.MAX_DAY_HOURS Then
                    problem = $"Overtime must be between 0 and {AttendanceEngine.MAX_DAY_HOURS:0} hours."
                End If
        End Select
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
        Dim item As DailyAttendanceRow = DirectCast(gridRow.Tag, DailyAttendanceRow)
        Select Case _grid.Columns(e.ColumnIndex).Name
            Case COL_STATUS
                item.Status = AttendanceCodes.Normalize(CellText(gridRow, COL_STATUS))
                gridRow.Cells(COL_STATUS).Value = item.Status
            Case COL_IN, COL_OUT
                ApplyTimes(gridRow, item)
            Case COL_OT
                Dim hours As Decimal
                ParseOt(CellText(gridRow, COL_OT), hours)
                item.OtHours = hours
                gridRow.Cells(COL_OT).Value = hours.ToString(OT_FORMAT)
        End Select
        MarkDirty()
    End Sub

    Private Sub ApplyTimes(gridRow As DataGridViewRow, item As DailyAttendanceRow)
        Dim inTime As TimeSpan
        Dim outTime As TimeSpan
        Dim hasIn As Boolean = UiKit.TryParseTime(CellText(gridRow, COL_IN), inTime)
        Dim hasOut As Boolean = UiKit.TryParseTime(CellText(gridRow, COL_OUT), outTime)
        item.InTime = If(hasIn, New Nullable(Of TimeSpan)(inTime), Nothing)
        item.OutTime = If(hasOut, New Nullable(Of TimeSpan)(outTime), Nothing)
        gridRow.Cells(COL_IN).Value = UiKit.FormatTime(item.InTime)
        gridRow.Cells(COL_OUT).Value = UiKit.FormatTime(item.OutTime)
        If Not (hasIn AndAlso hasOut) Then
            Return
        End If
        If String.IsNullOrEmpty(item.Status) Then
            item.Status = AttendanceCodes.PRESENT
            gridRow.Cells(COL_STATUS).Value = item.Status
        End If
        If item.Status = AttendanceCodes.PRESENT OrElse item.Status = AttendanceCodes.HALF_DAY Then
            Dim settings As New OvertimeSettings() With {.ShiftHours = _nudShift.Value}
            item.OtHours = AttendanceEngine.OvertimeHours(inTime, outTime, settings)
            gridRow.Cells(COL_OT).Value = item.OtHours.ToString(OT_FORMAT)
        End If
    End Sub

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
            Dim item As DailyAttendanceRow = DirectCast(_grid.Rows(index).Tag, DailyAttendanceRow)
            If item.EmpCode.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
               item.EmployeeName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 Then
                _grid.CurrentCell = _grid.Rows(index).Cells(COL_STATUS)
                _grid.Focus()
                Return
            End If
        Next
        UiKit.ShowProblem(_lblStatus, "No employee matches.")
    End Sub

    Private Sub OnDateChanged(sender As Object, e As EventArgs)
        If _loading Then
            Return
        End If
        If _dirty AndAlso Not UiKit.Confirm(Me, "Discard the unsaved attendance changes and change the date?") Then
            _loading = True
            _dtpDate.Value = _loadedDate
            _loading = False
            Return
        End If
        LoadDay()
    End Sub

    Private Sub OnLoadClick(sender As Object, e As EventArgs)
        ReloadWithConfirm()
    End Sub

    Private Sub OnFillClick(sender As Object, e As EventArgs)
        Try
            Dim suggestion As String = _service.SuggestStatus(_loadedDate)
            For Each gridRow As DataGridViewRow In _grid.Rows
                Dim item As DailyAttendanceRow = DirectCast(gridRow.Tag, DailyAttendanceRow)
                If item.IsLocked OrElse Not String.IsNullOrEmpty(item.Status) Then
                    Continue For
                End If
                item.Status = suggestion
                gridRow.Cells(COL_STATUS).Value = suggestion
            Next
            MarkDirty()
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub OnSaveClick(sender As Object, e As EventArgs)
        SaveDay()
    End Sub

    Private Sub OnCloseClick(sender As Object, e As EventArgs)
        Close()
    End Sub

End Class
