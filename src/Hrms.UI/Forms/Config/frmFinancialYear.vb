Imports System.Drawing
Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

Public Class frmFinancialYear
    Inherits BaseForm

    Private Const FORM_WIDTH As Integer = 820
    Private Const FORM_HEIGHT As Integer = 480
    Private Const LIST_WIDTH As Integer = 440
    Private Const LABEL_COLUMN_WIDTH As Integer = 120

    Private ReadOnly _service As FinancialYearService
    Private ReadOnly _canEdit As Boolean
    Private ReadOnly _split As New SplitContainer()
    Private ReadOnly _grid As New DataGridView()
    Private ReadOnly _nudStartYear As New NumericUpDown()
    Private ReadOnly _lblPreview As New Label()
    Private ReadOnly _lblStatus As New Label()
    Private ReadOnly _btnCreate As Button = UiKit.CreateButton("Create", ButtonKind.Primary)
    Private ReadOnly _btnSetCurrent As Button = UiKit.CreateButton("Set current", ButtonKind.Normal)
    Private ReadOnly _btnClose As Button = UiKit.CreateButton("Close", ButtonKind.Normal)
    Private _items As New List(Of FinancialYearDto)()

    Public Sub New(service As FinancialYearService, user As IUserContext)
        MyBase.New()
        _service = service
        _canEdit = user.CanAccess(AccessModules.FINANCIAL_YEAR, AccessActions.EDIT)
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        Text = "Financial Year Setup"
        ClientSize = New Size(FORM_WIDTH, FORM_HEIGHT)

        _split.Dock = DockStyle.Fill
        _split.FixedPanel = FixedPanel.Panel1

        UiKit.StyleGrid(_grid, True)
        _grid.Dock = DockStyle.Fill
        _grid.ReadOnly = True
        UiKit.AddTextColumn(_grid, "colLabel", "Financial year", 30.0F, True, False)
        UiKit.AddTextColumn(_grid, "colStart", "Start", 25.0F, True, False)
        UiKit.AddTextColumn(_grid, "colEnd", "End", 25.0F, True, False)
        UiKit.AddTextColumn(_grid, "colCurrent", "Current", 20.0F, True, False)
        _split.Panel1.Controls.Add(_grid)

        Dim details As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 2, .Padding = New Padding(12)}
        details.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, LABEL_COLUMN_WIDTH))
        details.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        _nudStartYear.Minimum = FinancialYearRules.MIN_START_YEAR
        _nudStartYear.Maximum = FinancialYearRules.MAX_START_YEAR
        _nudStartYear.Value = Date.Today.Year
        _nudStartYear.Width = 90
        _lblPreview.AutoSize = True
        details.Controls.Add(UiKit.RequiredLabel("Start year"), 0, 0)
        details.Controls.Add(_nudStartYear, 1, 0)
        details.Controls.Add(New Label() With {.Text = "Period", .AutoSize = True, .Margin = New Padding(0, 6, 0, 0)}, 0, 1)
        details.Controls.Add(_lblPreview, 1, 1)
        Dim page As New TabPage("New financial year")
        page.Controls.Add(details)
        Dim tabs As New TabControl() With {.Dock = DockStyle.Fill}
        tabs.TabPages.Add(page)
        _split.Panel2.Controls.Add(tabs)

        Dim bottomBar As New Panel() With {.Dock = DockStyle.Bottom, .Height = UiKit.FOOTER_HEIGHT}
        Dim buttons As New FlowLayoutPanel() With {
            .Dock = DockStyle.Right, .AutoSize = True, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)
        }
        buttons.Controls.Add(_btnClose)
        buttons.Controls.Add(_btnSetCurrent)
        buttons.Controls.Add(_btnCreate)
        _lblStatus.Dock = DockStyle.Fill
        _lblStatus.TextAlign = ContentAlignment.MiddleLeft
        _lblStatus.Padding = New Padding(8, 0, 0, 0)
        bottomBar.Controls.Add(_lblStatus)
        bottomBar.Controls.Add(buttons)

        Controls.Add(_split)
        Controls.Add(bottomBar)

        AddHandler _nudStartYear.ValueChanged, AddressOf OnStartYearChanged
        AddHandler _btnCreate.Click, AddressOf OnCreateClick
        AddHandler _btnSetCurrent.Click, AddressOf OnSetCurrentClick
        AddHandler _btnClose.Click, AddressOf OnCloseClick
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        _split.SplitterDistance = LIST_WIDTH
        _btnCreate.Enabled = _canEdit
        _btnSetCurrent.Enabled = _canEdit
        _nudStartYear.Enabled = _canEdit
        If Not _canEdit Then
            UiKit.ShowProblem(_lblStatus, "View only. You do not have permission to change financial years.")
        End If
        UpdatePreview()
        LoadList()
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.F5
                LoadList()
                Return True
            Case Keys.Control Or Keys.S
                CreateYear()
                Return True
            Case Keys.Escape
                Close()
                Return True
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Private Sub LoadList()
        Try
            _items = _service.List()
            _grid.Rows.Clear()
            For Each item As FinancialYearDto In _items
                Dim index As Integer = _grid.Rows.Add(item.Label, item.StartDate.ToString(UiKit.DATE_FORMAT),
                                                      item.EndDate.ToString(UiKit.DATE_FORMAT), If(item.IsCurrent, "Yes", String.Empty))
                _grid.Rows(index).Tag = item
                If item.IsCurrent Then
                    _grid.Rows(index).DefaultCellStyle.Font = New Font(_grid.Font, FontStyle.Bold)
                End If
            Next
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub UpdatePreview()
        Dim startYear As Integer = CInt(_nudStartYear.Value)
        _lblPreview.Text = $"{FinancialYearRules.BuildLabel(startYear)}   " &
            $"({FinancialYearRules.StartOf(startYear).ToString(UiKit.DATE_FORMAT)} to {FinancialYearRules.EndOf(startYear).ToString(UiKit.DATE_FORMAT)})"
    End Sub

    Private Sub CreateYear()
        If Not _canEdit Then
            Return
        End If
        Try
            _service.Create(CInt(_nudStartYear.Value))
            UiKit.ShowSuccess(Me, "Financial year created.", _lblStatus)
            LoadList()
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub OnStartYearChanged(sender As Object, e As EventArgs)
        UpdatePreview()
    End Sub

    Private Sub OnCreateClick(sender As Object, e As EventArgs)
        CreateYear()
    End Sub

    Private Sub OnSetCurrentClick(sender As Object, e As EventArgs)
        If _grid.CurrentRow Is Nothing Then
            UiKit.ShowInfo(Me, "Select a financial year in the list first.")
            Return
        End If
        Dim item As FinancialYearDto = DirectCast(_grid.CurrentRow.Tag, FinancialYearDto)
        If item.IsCurrent Then
            Return
        End If
        If Not UiKit.Confirm(Me, $"Make {item.Label} the current financial year?") Then
            Return
        End If
        Try
            _service.SetCurrent(item.Id)
            UiKit.ShowSuccess(Me, $"{item.Label} is now the current financial year.", _lblStatus)
            LoadList()
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub OnCloseClick(sender As Object, e As EventArgs)
        Close()
    End Sub

End Class
