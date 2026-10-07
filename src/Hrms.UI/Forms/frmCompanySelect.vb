Imports System.Windows.Forms
Imports Hrms.Models

''' <summary>Pick the company to work in. Shown after sign-in when the user has more than one, and from File > Switch company.</summary>
Friend Class frmCompanySelect
    Inherits BaseForm

    Private ReadOnly _list As New ListBox() With {.Dock = DockStyle.Fill, .IntegralHeight = False}
    Private ReadOnly _btnOk As New Button() With {.Text = "OK", .DialogResult = DialogResult.OK}
    Private ReadOnly _btnCancel As New Button() With {.Text = "Cancel", .DialogResult = DialogResult.Cancel}

    Public Sub New(companies As IEnumerable(Of CompanyInfo), current As CompanyInfo)
        Text = "Select company"
        StartPosition = FormStartPosition.CenterParent
        DialogLayout.StyleDialog(Me, 380, 360)
        AcceptButton = _btnOk
        CancelButton = _btnCancel

        For Each company In companies
            _list.Items.Add(company)
            If current IsNot Nothing AndAlso company.CompanyId = current.CompanyId Then
                _list.SelectedItem = company
            End If
        Next
        If _list.SelectedIndex < 0 AndAlso _list.Items.Count > 0 Then
            _list.SelectedIndex = 0
        End If
        AddHandler _list.DoubleClick, Sub(sender, e) AcceptSelection()

        Dim buttons = DialogLayout.CreateButtonBar(_btnOk, _btnCancel)
        buttons.Dock = DockStyle.Bottom
        buttons.Padding = New Padding(12, 8, 12, 8)

        Controls.Add(_list)
        Controls.Add(buttons)
        Controls.Add(DialogLayout.CreateHeader("Select company"))
    End Sub

    ''' <summary>The chosen company, or Nothing when nothing is selected.</summary>
    Public ReadOnly Property SelectedCompany As CompanyInfo
        Get
            Return TryCast(_list.SelectedItem, CompanyInfo)
        End Get
    End Property

    Private Sub AcceptSelection()
        If _list.SelectedItem IsNot Nothing Then
            DialogResult = DialogResult.OK
        End If
    End Sub

End Class
