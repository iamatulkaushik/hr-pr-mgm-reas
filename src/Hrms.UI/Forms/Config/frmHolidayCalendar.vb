Imports System.Drawing
Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

Public Class frmHolidayCalendar
    Inherits BaseForm

    Private Const FORM_WIDTH As Integer = 960
    Private Const FORM_HEIGHT As Integer = 540
    Private Const LIST_WIDTH As Integer = 520
    Private Const LABEL_COLUMN_WIDTH As Integer = 110
    Private Const NAME_MAX_LENGTH As Integer = 100
    Private Const DAY_NAME_FORMAT As String = "dddd"

    Private ReadOnly _holidays As HolidayService
    Private ReadOnly _years As FinancialYearService
    Private ReadOnly _canEdit As Boolean
    Private ReadOnly _split As New SplitContainer()
    Private ReadOnly _cboYear As New ComboBox()
    Private ReadOnly _grid As New DataGridView()
    Private ReadOnly _dtpDate As New DateTimePicker()
    Private ReadOnly _txtName As New TextBox()
    Private ReadOnly _cboType As New ComboBox()
    Private ReadOnly _lblStatus As New Label()
    Private ReadOnly _btnNew As Button = UiKit.CreateButton("New", ButtonKind.Normal)
    Private ReadOnly _btnSave As Button = UiKit.CreateButton("Save", ButtonKind.Primary)
    Private ReadOnly _btnDelete As Button = UiKit.CreateButton("Remove", ButtonKind.Danger)
    Private ReadOnly _btnCancel As Button = UiKit.CreateButton("Cancel", ButtonKind.Normal)
    Private _items As New List(Of HolidayDto)()
    Private _selectedId As Integer
    Private _loading As Boolean

    Public Sub New(holidays As HolidayService, years As FinancialYearService, user As IUserContext)
        MyBase.New()
        _holidays = holidays
        _years = years
        _canEdit = user.CanAccess(AccessModules.HOLIDAY, AccessActions.EDIT)
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        Text = "Holiday Calendar"
        ClientSize = New Size(FORM_WIDTH, FORM_HEIGHT)

        _split.Dock = DockStyle.Fill
        _split.FixedPanel = FixedPanel.Panel1

        UiKit.StyleGrid(_grid, True)
        _grid.Dock = DockStyle.Fill
        _grid.ReadOnly = True
        UiKit.AddTextColumn(_grid, "colDate", "Date", 20.0F, True, False)
        UiKit.AddTextColumn(_grid, "colDay", "Day", 20.0F, True, False)
        UiKit.AddTextColumn(_grid, "colName", "Holiday", 40.0F, True, False)
        UiKit.AddTextColumn(_grid, "colType", "Type", 20.0F, True, False)
        _split.Panel1.Controls.Add(_grid)

        _dtpDate.Format = DateTimePickerFormat.Custom
        _dtpDate.CustomFormat = UiKit.DATE_FORMAT
        _dtpDate.Width = 120
        _txtName.MaxLength = NAME_MAX_LENGTH
        _txtName.Dock = DockStyle.Top
        _cboType.DropDownStyle = ComboBoxStyle.DropDownList
        _cboType.Items.AddRange(HolidayTypes.All)
        _cboType.SelectedIndex = 0
        Dim details As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 2, .Padding = New Padding(12)}
        details.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, LABEL_COLUMN_WIDTH))
        details.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        details.Controls.Add(UiKit.RequiredLabel("Date"), 0, 0)
        details.Controls.Add(_dtpDate, 1, 0)
        details.Controls.Add(UiKit.RequiredLabel("Holiday name"), 0, 1)
        details.Controls.Add(_txtName, 1, 1)
        details.Controls.Add(UiKit.RequiredLabel("Type"), 0, 2)
        details.Controls.Add(_cboType, 1, 2)
        Dim page As New TabPage("Details")
        page.Controls.Add(details)
        Dim tabs As New TabControl() With {.Dock = DockStyle.Fill}
        tabs.TabPages.Add(page)
        _split.Panel2.Controls.Add(tabs)

        Dim topBar As New Panel() With {.Dock = DockStyle.Top, .Height = UiKit.BAR_HEIGHT}
        topBar.Controls.Add(New Label() With {.Text = "Financial year:", .AutoSize = True, .Left = 8, .Top = 14})
        _cboYear.DropDownStyle = ComboBoxStyle.DropDownList
        _cboYear.SetBounds(110, 10, 140, 24)
        topBar.Controls.Add(_cboYear)

        Dim bottomBar As New Panel() With {.Dock = DockStyle.Bottom, .Height = UiKit.FOOTER_HEIGHT}
        Dim buttons As New FlowLayoutPanel() With {
            .Dock = DockStyle.Right, .AutoSize = True, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)
        }
        buttons.Controls.Add(_btnCancel)
        buttons.Controls.Add(_btnDelete)
        buttons.Controls.Add(_btnSave)
        buttons.Controls.Add(_btnNew)
        _lblStatus.Dock = DockStyle.Fill
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft
        _lblStatus.Padding = New Padding(8, 0, 0, 0)
        bottomBar.Controls.Add(_lblStatus)
        bottomBar.Controls.Add(buttons)

        Controls.Add(_split)
        Controls.Add(topBar)
        Controls.Add(bottomBar)

        AddHandler _cboYear.SelectedIndexChanged, AddressOf OnYearChanged
        AddHandler _grid.SelectionChanged, AddressOf OnGridSelectionChanged
        AddHandler _btnNew.Click, AddressOf OnNewClick
        AddHandler _btnSave.Click, AddressOf OnSaveClick
        AddHandler _btnDelete.Click, AddressOf OnDeleteClick
        AddHandler _btnCancel.Click, AddressOf OnCancelClick
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        _split.SplitterDistance = LIST_WIDTH
        If Not _canEdit Then
            _dtpDate.Enabled = False
            _txtName.ReadOnly = True
            _cboType.Enabled = False
            _btnNew.Enabled = False
            _btnSave.Enabled = False
            _btnDelete.Enabled = False
            _btnCancel.Enabled = False
            UiKit.ShowProblem(_lblStatus, "View only. You do not have permission to change holidays.")
        End If
        LoadYears()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.F5
                LoadHolidays(_selectedId)
                Return True
            Case Keys.Control Or Keys.S
                SaveCurrent()
                Return True
            Case Keys.Escape
                Close()
                Return True
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Function SelectedYear() As FinancialYearDto
        Return TryCast(_cboYear.SelectedItem, FinancialYearDto)
    End Function

    Private Sub LoadYears()
        Try
            Dim years As List(Of FinancialYearDto) = _years.List()
            _loading = True
            _cboYear.DisplayMember = "Label"
            _cboYear.DataSource = years
            _loading = False
            If years.Count = 0 Then
                UiKit.ShowInfo(Me, "Create a financial year first (Masters > Financial Year).")
                _btnNew.Enabled = False
                _btnSave.Enabled = False
                Return
            End If
            Dim current As FinancialYearDto = years.FirstOrDefault(Function(y) y.IsCurrent)
            _cboYear.SelectedItem = If(current, years(0))
            ApplyYearRange()
            LoadHolidays(0)
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub ApplyYearRange()
        Dim year As FinancialYearDto = SelectedYear()
        If year Is Nothing Then
            Return
        End If
        _dtpDate.MinDate = DateTimePicker.MinimumDateTime
        _dtpDate.MaxDate = DateTimePicker.MaximumDateTime
        _dtpDate.MinDate = year.StartDate
        _dtpDate.MaxDate = year.EndDate
    End Sub

    Private Sub LoadHolidays(selectId As Integer)
        Dim year As FinancialYearDto = SelectedYear()
        If year Is Nothing Then
            Return
        End If
        Try
            _items = _holidays.List(year.StartDate, year.EndDate)
            _loading = True
            _grid.Rows.Clear()
            For Each item As HolidayDto In _items
                Dim index As Integer = _grid.Rows.Add(item.HolidayDate.ToString(UiKit.DATE_FORMAT),
                                                      item.HolidayDate.ToString(DAY_NAME_FORMAT), item.Name, item.HolidayType)
                _grid.Rows(index).Tag = item
            Next
            _loading = False
            SelectById(selectId)
        Catch ex As BusinessRuleException
            _loading = False
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub SelectById(selectId As Integer)
        If selectId <> 0 Then
            For Each row As DataGridViewRow In _grid.Rows
                If DirectCast(row.Tag, HolidayDto).Id = selectId Then
                    _grid.CurrentCell = row.Cells(0)
                    ShowSelected()
                    Return
                End If
            Next
        End If
        StartNew()
    End Sub

    Private Sub ShowSelected()
        If _grid.CurrentRow Is Nothing Then
            StartNew()
            Return
        End If
        Dim item As HolidayDto = DirectCast(_grid.CurrentRow.Tag, HolidayDto)
        _selectedId = item.Id
        _dtpDate.Value = item.HolidayDate
        _txtName.Text = item.Name
        _cboType.SelectedItem = item.HolidayType
    End Sub

    Private Sub StartNew()
        _selectedId = 0
        _txtName.Clear()
        _cboType.SelectedIndex = 0
        Dim year As FinancialYearDto = SelectedYear()
        If year IsNot Nothing Then
            _dtpDate.Value = If(Date.Today >= year.StartDate AndAlso Date.Today <= year.EndDate, Date.Today, year.StartDate)
        End If
        _txtName.Focus()
    End Sub

    Private Sub SaveCurrent()
        Dim year As FinancialYearDto = SelectedYear()
        If Not _canEdit OrElse year Is Nothing Then
            Return
        End If
        Dim item As New HolidayDto() With {
            .Id = _selectedId,
            .HolidayDate = _dtpDate.Value.Date,
            .Name = _txtName.Text.Trim(),
            .HolidayType = Convert.ToString(_cboType.SelectedItem)
        }
        Try
            Dim savedId As Integer = _holidays.Save(item, year.StartDate, year.EndDate)
            UiKit.ShowSuccess(Me, "Holiday saved.", _lblStatus)
            LoadHolidays(savedId)
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub OnYearChanged(sender As Object, e As EventArgs)
        If _loading Then
            Return
        End If
        ApplyYearRange()
        LoadHolidays(0)
    End Sub

    Private Sub OnGridSelectionChanged(sender As Object, e As EventArgs)
        If Not _loading Then
            ShowSelected()
        End If
    End Sub

    Private Sub OnNewClick(sender As Object, e As EventArgs)
        _grid.ClearSelection()
        StartNew()
    End Sub

    Private Sub OnSaveClick(sender As Object, e As EventArgs)
        SaveCurrent()
    End Sub

    Private Sub OnCancelClick(sender As Object, e As EventArgs)
        ShowSelected()
    End Sub

    Private Sub OnDeleteClick(sender As Object, e As EventArgs)
        If _selectedId = 0 Then
            UiKit.ShowInfo(Me, "Select a holiday in the list first.")
            Return
        End If
        If Not UiKit.Confirm(Me, $"Remove the holiday '{_txtName.Text.Trim()}'?") Then
            Return
        End If
        Try
            _holidays.Deactivate(_selectedId)
            UiKit.ShowSuccess(Me, "Holiday removed.", _lblStatus)
            LoadHolidays(0)
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

End Class
