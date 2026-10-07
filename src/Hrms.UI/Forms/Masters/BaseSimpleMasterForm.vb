Imports System.Drawing
Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

' Standard master layout: search bar on top, list on the left, detail tab on the right, buttons at the bottom.
Public MustInherit Class BaseSimpleMasterForm
    Inherits BaseForm

    Private Const FORM_WIDTH As Integer = 900
    Private Const FORM_HEIGHT As Integer = 520
    Private Const LIST_WIDTH As Integer = 380
    Private Const LABEL_COLUMN_WIDTH As Integer = 110
    Private Const NAME_MAX_LENGTH As Integer = 100
    Private Const STATUS_ACTIVE As String = "Active"
    Private Const STATUS_INACTIVE As String = "Inactive"

    Private ReadOnly _canEdit As Boolean
    Private ReadOnly _split As New SplitContainer()
    Private ReadOnly _txtSearch As New TextBox()
    Private ReadOnly _chkShowInactive As New CheckBox()
    Private ReadOnly _grid As New DataGridView()
    Private ReadOnly _txtName As New TextBox()
    Private ReadOnly _chkActive As New CheckBox()
    Private ReadOnly _lblStatus As New Label()
    Private ReadOnly _btnNew As Button = UiKit.CreateButton("New", ButtonKind.Normal)
    Private ReadOnly _btnSave As Button = UiKit.CreateButton("Save", ButtonKind.Primary)
    Private ReadOnly _btnCancel As Button = UiKit.CreateButton("Cancel", ButtonKind.Normal)
    Private _items As New List(Of NamedMasterDto)()
    Private _selectedId As Integer
    Private _loading As Boolean

    Protected MustOverride ReadOnly Property EntityName As String

    Protected MustOverride Function LoadItems(includeInactive As Boolean) As List(Of NamedMasterDto)

    Protected MustOverride Function SaveItem(item As NamedMasterDto) As Integer

    Protected Sub New(canEdit As Boolean)
        MyBase.New()
        _canEdit = canEdit
        BuildLayout()
    End Sub

    Private Sub BuildLayout()
        ClientSize = New Size(FORM_WIDTH, FORM_HEIGHT)

        _split.Dock = DockStyle.Fill
        _split.FixedPanel = FixedPanel.Panel1

        UiKit.StyleGrid(_grid, True)
        _grid.Dock = DockStyle.Fill
        _grid.ReadOnly = True
        UiKit.AddTextColumn(_grid, "colName", "Name", 70.0F, True, False)
        UiKit.AddTextColumn(_grid, "colStatus", "Status", 30.0F, True, False)
        _split.Panel1.Controls.Add(_grid)

        Dim details As New TableLayoutPanel() With {.Dock = DockStyle.Fill, .ColumnCount = 2, .Padding = New Padding(12)}
        details.ColumnStyles.Add(New ColumnStyle(SizeType.Absolute, LABEL_COLUMN_WIDTH))
        details.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        details.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        details.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        _txtName.MaxLength = NAME_MAX_LENGTH
        _txtName.Dock = DockStyle.Top
        _chkActive.Text = STATUS_ACTIVE
        _chkActive.Checked = True
        _chkActive.AutoSize = True
        details.Controls.Add(UiKit.RequiredLabel("Name"), 0, 0)
        details.Controls.Add(_txtName, 1, 0)
        details.Controls.Add(New Label() With {.Text = "Status", .AutoSize = True, .Margin = New Padding(0, 6, 0, 0)}, 0, 1)
        details.Controls.Add(_chkActive, 1, 1)
        Dim page As New TabPage("Details")
        page.Controls.Add(details)
        Dim tabs As New TabControl() With {.Dock = DockStyle.Fill}
        tabs.TabPages.Add(page)
        _split.Panel2.Controls.Add(tabs)

        Dim topBar As New Panel() With {.Dock = DockStyle.Top, .Height = UiKit.BAR_HEIGHT}
        topBar.Controls.Add(New Label() With {.Text = "Find (F3):", .AutoSize = True, .Left = 8, .Top = 14})
        _txtSearch.SetBounds(80, 10, 220, 24)
        _chkShowInactive.Text = "Show inactive"
        _chkShowInactive.AutoSize = True
        _chkShowInactive.Left = 320
        _chkShowInactive.Top = 12
        topBar.Controls.Add(_txtSearch)
        topBar.Controls.Add(_chkShowInactive)

        Dim bottomBar As New Panel() With {.Dock = DockStyle.Bottom, .Height = UiKit.FOOTER_HEIGHT}
        Dim buttons As New FlowLayoutPanel() With {
            .Dock = DockStyle.Right, .AutoSize = True, .FlowDirection = FlowDirection.RightToLeft, .Padding = New Padding(8)
        }
        buttons.Controls.Add(_btnCancel)
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

        AddHandler _grid.SelectionChanged, AddressOf OnGridSelectionChanged
        AddHandler _txtSearch.TextChanged, AddressOf OnSearchChanged
        AddHandler _chkShowInactive.CheckedChanged, AddressOf OnShowInactiveChanged
        AddHandler _btnNew.Click, AddressOf OnNewClick
        AddHandler _btnSave.Click, AddressOf OnSaveClick
        AddHandler _btnCancel.Click, AddressOf OnCancelClick
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        Text = $"{EntityName} Master"
        _split.SplitterDistance = LIST_WIDTH
        If Not _canEdit Then
            _txtName.ReadOnly = True
            _chkActive.Enabled = False
            _btnSave.Enabled = False
            _btnNew.Enabled = False
            _btnCancel.Enabled = False
            UiKit.ShowProblem(_lblStatus, "View only. You do not have permission to change this master.")
        End If
        LoadList(0)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.F3
                _txtSearch.Focus()
                Return True
            Case Keys.F5
                LoadList(_selectedId)
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

    Private Sub LoadList(selectId As Integer)
        Try
            _items = LoadItems(_chkShowInactive.Checked)
            FillGrid(selectId)
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub FillGrid(selectId As Integer)
        _loading = True
        _grid.Rows.Clear()
        Dim filter As String = _txtSearch.Text.Trim()
        For Each item As NamedMasterDto In _items
            If filter.Length > 0 AndAlso item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0 Then
                Continue For
            End If
            Dim index As Integer = _grid.Rows.Add(item.Name, If(item.IsActive, STATUS_ACTIVE, STATUS_INACTIVE))
            _grid.Rows(index).Tag = item
            If Not item.IsActive Then
                _grid.Rows(index).DefaultCellStyle.ForeColor = Color.Gray
            End If
        Next
        _loading = False
        SelectById(selectId)
    End Sub

    Private Sub SelectById(selectId As Integer)
        For Each row As DataGridViewRow In _grid.Rows
            If DirectCast(row.Tag, NamedMasterDto).Id = selectId AndAlso selectId <> 0 Then
                _grid.CurrentCell = row.Cells(0)
                ShowSelected()
                Return
            End If
        Next
        If selectId = 0 AndAlso _grid.Rows.Count > 0 Then
            _grid.CurrentCell = _grid.Rows(0).Cells(0)
            ShowSelected()
        Else
            StartNew()
        End If
    End Sub

    Private Sub ShowSelected()
        If _grid.CurrentRow Is Nothing Then
            StartNew()
            Return
        End If
        Dim item As NamedMasterDto = DirectCast(_grid.CurrentRow.Tag, NamedMasterDto)
        _selectedId = item.Id
        _txtName.Text = item.Name
        _chkActive.Checked = item.IsActive
    End Sub

    Private Sub StartNew()
        _selectedId = 0
        _txtName.Clear()
        _chkActive.Checked = True
        _txtName.Focus()
    End Sub

    Private Sub SaveCurrent()
        If Not _canEdit Then
            Return
        End If
        Dim item As New NamedMasterDto() With {
            .Id = _selectedId,
            .Name = _txtName.Text.Trim(),
            .IsActive = _chkActive.Checked
        }
        Try
            Dim savedId As Integer = SaveItem(item)
            UiKit.ShowSuccess(Me, $"{EntityName} saved.", _lblStatus)
            LoadList(savedId)
        Catch ex As BusinessRuleException
            UiKit.ShowWarning(Me, ex.Message)
        End Try
    End Sub

    Private Sub OnGridSelectionChanged(sender As Object, e As EventArgs)
        If Not _loading Then
            ShowSelected()
        End If
    End Sub

    Private Sub OnSearchChanged(sender As Object, e As EventArgs)
        FillGrid(_selectedId)
    End Sub

    Private Sub OnShowInactiveChanged(sender As Object, e As EventArgs)
        LoadList(_selectedId)
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

End Class
