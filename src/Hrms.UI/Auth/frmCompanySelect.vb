Imports System.ComponentModel
Imports Hrms.Models
Imports Hrms.Models.Company

''' <summary>
''' Choose the active company (FR-3, F2). Standard layout: search bar on top, list on the left,
''' detail tab on the right, buttons at the bottom. Returns the choice on DialogResult.OK.
''' </summary>
Public Class frmCompanySelect

    Private ReadOnly _all As IReadOnlyList(Of CompanyInfo)
    Private ReadOnly _currentCompanyId As Integer?
    Private _selected As CompanyInfo

    Public Sub New(companies As IReadOnlyList(Of CompanyInfo), currentCompanyId As Integer?)
        InitializeComponent()
        _all = companies
        _currentCompanyId = currentCompanyId
    End Sub

    Public ReadOnly Property SelectedCompany As CompanyInfo
        Get
            Return _selected
        End Get
    End Property

    Private Sub frmCompanySelect_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ConfigureGrid()
        BindList(String.Empty)
        SelectCurrent()
        dgvCompanies.Select()
    End Sub

    Private Sub ConfigureGrid()
        dgvCompanies.AutoGenerateColumns = False
        dgvCompanies.Columns.Add(New DataGridViewTextBoxColumn With {
            .DataPropertyName = NameOf(CompanyInfo.Name), .HeaderText = "Company", .Width = 220})
        dgvCompanies.Columns.Add(New DataGridViewTextBoxColumn With {
            .DataPropertyName = NameOf(CompanyInfo.Pan), .HeaderText = "PAN", .Width = 110})
    End Sub

    Private Sub BindList(filter As String)
        Dim rows As IEnumerable(Of CompanyInfo) = _all
        If filter.Length > 0 Then
            rows = _all.Where(Function(c) c.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 OrElse
                                          c.Pan.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
        End If
        dgvCompanies.DataSource = New BindingList(Of CompanyInfo)(rows.ToList())
        ShowDetails()
    End Sub

    Private Sub SelectCurrent()
        If Not _currentCompanyId.HasValue Then Return
        For Each row As DataGridViewRow In dgvCompanies.Rows
            If DirectCast(row.DataBoundItem, CompanyInfo).CompanyID = _currentCompanyId.Value Then
                row.Selected = True
                dgvCompanies.CurrentCell = row.Cells(0)
                Exit For
            End If
        Next
    End Sub

    Private Function CurrentRowCompany() As CompanyInfo
        If dgvCompanies.CurrentRow Is Nothing Then Return Nothing
        Return TryCast(dgvCompanies.CurrentRow.DataBoundItem, CompanyInfo)
    End Function

    Private Sub ShowDetails()
        Dim company As CompanyInfo = CurrentRowCompany()
        lblNameVal.Text = If(company?.Name, String.Empty)
        lblAddressVal.Text = If(company?.Address, String.Empty)
        lblPanVal.Text = If(company?.Pan, String.Empty)
        lblTanVal.Text = If(company?.Tan, String.Empty)
        lblEpfVal.Text = If(company?.EpfRegn, String.Empty)
        lblEsiVal.Text = If(company?.EsiRegn, String.Empty)
    End Sub

    Private Sub txtSearch_TextChanged(sender As Object, e As EventArgs) Handles txtSearch.TextChanged
        BindList(txtSearch.Text.Trim())
    End Sub

    Private Sub dgvCompanies_SelectionChanged(sender As Object, e As EventArgs) Handles dgvCompanies.SelectionChanged
        ShowDetails()
    End Sub

    Private Sub dgvCompanies_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs) Handles dgvCompanies.CellDoubleClick
        If e.RowIndex >= 0 Then ChooseCurrent()
    End Sub

    Protected Overrides Sub FindRequested()
        txtSearch.Select()
    End Sub

    Private Sub btnSelect_Click(sender As Object, e As EventArgs) Handles btnSelect.Click
        ChooseCurrent()
    End Sub

    Private Sub ChooseCurrent()
        Dim company As CompanyInfo = CurrentRowCompany()
        If company Is Nothing Then
            Msg.Info(Me, "Select a company from the list.")
            Return
        End If
        _selected = company
        DialogResult = DialogResult.OK
    End Sub

End Class
