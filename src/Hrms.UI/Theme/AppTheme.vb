Imports System.Drawing
Imports System.Windows.Forms

Public NotInheritable Class AppTheme

    Private Const FONT_NAME As String = "Segoe UI"
    Private Const FONT_SIZE As Single = 9.5F

    Public Shared ReadOnly Primary As Color = ColorTranslator.FromHtml("#1F4E79")
    Public Shared ReadOnly Accent As Color = ColorTranslator.FromHtml("#2E8B57")
    Public Shared ReadOnly Danger As Color = ColorTranslator.FromHtml("#B22222")
    Public Shared ReadOnly Background As Color = ColorTranslator.FromHtml("#F4F6F8")
    Public Shared ReadOnly GridAltRow As Color = ColorTranslator.FromHtml("#EEF3F8")
    Public Shared ReadOnly InputFocus As Color = ColorTranslator.FromHtml("#FFF8DC")

    Public Shared ReadOnly BaseFont As New Font(FONT_NAME, FONT_SIZE, FontStyle.Regular, GraphicsUnit.Point)

    Private Sub New()
    End Sub

    Public Shared Sub StyleGrid(grid As DataGridView)
        grid.BackgroundColor = Color.White
        grid.EnableHeadersVisualStyles = False
        grid.RowHeadersVisible = False
        grid.AllowUserToAddRows = False
        grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltRow
        grid.ColumnHeadersDefaultCellStyle.BackColor = Primary
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
    End Sub

End Class
