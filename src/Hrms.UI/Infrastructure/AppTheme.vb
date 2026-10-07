Imports System.Drawing
Imports System.Windows.Forms

''' <summary>Theme from DESIGN.md: Segoe UI 9.5 pt, navy primary, green accent, high contrast.</summary>
Public NotInheritable Class AppTheme

    Public Shared ReadOnly Primary As Color = ColorTranslator.FromHtml("#1F4E79")
    Public Shared ReadOnly Accent As Color = ColorTranslator.FromHtml("#2E8B57")
    Public Shared ReadOnly Danger As Color = ColorTranslator.FromHtml("#B22222")
    Public Shared ReadOnly Warning As Color = ColorTranslator.FromHtml("#B36B00")
    Public Shared ReadOnly Background As Color = ColorTranslator.FromHtml("#F4F6F8")
    Public Shared ReadOnly GridAltRow As Color = ColorTranslator.FromHtml("#EEF3F8")
    Public Shared ReadOnly InputFocus As Color = ColorTranslator.FromHtml("#FFF8DC")

    Public Const TAG_PRIMARY As String = "primary"
    Public Const TAG_ACCENT As String = "accent"
    Public Const TAG_DANGER As String = "danger"

    Private Const FONT_NAME As String = "Segoe UI"
    Private Const FONT_SIZE As Single = 9.5F

    Private Sub New()
    End Sub

    Public Shared Function BaseFont() As Font
        Return New Font(FONT_NAME, FONT_SIZE)
    End Function

    Public Shared Sub Apply(form As Form)
        form.Font = BaseFont()
        form.BackColor = Background
        ApplyToChildren(form)
    End Sub

    Private Shared Sub ApplyToChildren(parent As Control)
        For Each ctl As Control In parent.Controls
            If TypeOf ctl Is TextBox OrElse TypeOf ctl Is ComboBox OrElse
               TypeOf ctl Is DateTimePicker OrElse TypeOf ctl Is NumericUpDown Then
                WireFocusColor(ctl)
            ElseIf TypeOf ctl Is DataGridView Then
                StyleGrid(DirectCast(ctl, DataGridView))
            ElseIf TypeOf ctl Is Button Then
                StyleButton(DirectCast(ctl, Button))
            End If
            If ctl.HasChildren Then ApplyToChildren(ctl)
        Next
    End Sub

    Private Shared Sub WireFocusColor(ctl As Control)
        AddHandler ctl.Enter, Sub() ctl.BackColor = InputFocus
        AddHandler ctl.Leave, Sub() ctl.BackColor = SystemColors.Window
    End Sub

    Public Shared Sub StyleGrid(grid As DataGridView)
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Primary
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primary
        grid.AlternatingRowsDefaultCellStyle.BackColor = GridAltRow
        grid.DefaultCellStyle.SelectionBackColor = Primary
        grid.DefaultCellStyle.SelectionForeColor = Color.White
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.FixedSingle
    End Sub

    Private Shared Sub StyleButton(btn As Button)
        btn.FlatStyle = FlatStyle.Flat
        Dim back As Color
        Select Case CStr(btn.Tag)
            Case TAG_PRIMARY : back = Primary
            Case TAG_ACCENT : back = Accent
            Case TAG_DANGER : back = Danger
            Case Else
                btn.BackColor = Color.White
                btn.FlatAppearance.BorderColor = Primary
                Return
        End Select
        btn.BackColor = back
        btn.ForeColor = Color.White
        btn.FlatAppearance.BorderColor = back
    End Sub

End Class
