Imports System.Drawing
Imports System.Windows.Forms

''' <summary>Small helpers so the code-built dialogs (login, change password, ...) look the same.</summary>
Friend Module DialogLayout

    Public Function CreateGrid() As TableLayoutPanel
        Dim grid As New TableLayoutPanel() With {
            .Dock = DockStyle.Fill,
            .ColumnCount = 2,
            .Padding = New Padding(16, 12, 16, 12)
        }
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.AutoSize))
        grid.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 100.0F))
        Return grid
    End Function

    Public Sub AddField(grid As TableLayoutPanel, caption As String, field As Control)
        Dim row = grid.RowCount
        grid.RowCount = row + 1
        grid.RowStyles.Add(New RowStyle(SizeType.AutoSize))

        Dim label As New Label() With {
            .Text = caption,
            .AutoSize = True,
            .Anchor = AnchorStyles.Left,
            .Margin = New Padding(0, 6, 12, 6)
        }
        field.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        field.Margin = New Padding(0, 4, 0, 4)
        grid.Controls.Add(label, 0, row)
        grid.Controls.Add(field, 1, row)
    End Sub

    ''' <summary>Adds a control that spans both columns (message, hint, buttons).</summary>
    Public Sub AddFullRow(grid As TableLayoutPanel, control As Control)
        Dim row = grid.RowCount
        grid.RowCount = row + 1
        grid.RowStyles.Add(New RowStyle(SizeType.AutoSize))
        control.Anchor = AnchorStyles.Left Or AnchorStyles.Right
        grid.Controls.Add(control, 0, row)
        grid.SetColumnSpan(control, 2)
    End Sub

    Public Function CreateMessageLabel() As Label
        Return New Label() With {
            .AutoSize = False,
            .Height = 36,
            .ForeColor = AppTheme.Danger,
            .Margin = New Padding(0, 6, 0, 6)
        }
    End Function

    Public Function CreateHintLabel(text As String) As Label
        Return New Label() With {
            .Text = text,
            .AutoSize = False,
            .Height = 32,
            .Margin = New Padding(0, 4, 0, 4)
        }
    End Function

    Public Function CreateButtonBar(ParamArray buttons As Button()) As FlowLayoutPanel
        Dim bar As New FlowLayoutPanel() With {
            .FlowDirection = FlowDirection.RightToLeft,
            .AutoSize = True,
            .Margin = New Padding(0, 8, 0, 0)
        }
        For Each button In buttons
            button.AutoSize = True
            button.Padding = New Padding(10, 2, 10, 2)
            button.Margin = New Padding(8, 0, 0, 0)
            bar.Controls.Add(button)
        Next
        Return bar
    End Function

    Public Function CreateHeader(title As String) As Label
        Return New Label() With {
            .Text = title,
            .Dock = DockStyle.Top,
            .Height = 52,
            .BackColor = AppTheme.Primary,
            .ForeColor = Color.White,
            .Font = New Font("Segoe UI", 13.0F, FontStyle.Bold),
            .TextAlign = ContentAlignment.MiddleLeft,
            .Padding = New Padding(16, 0, 0, 0)
        }
    End Function

    Public Sub StyleDialog(form As Form, width As Integer, height As Integer)
        form.FormBorderStyle = FormBorderStyle.FixedDialog
        form.MaximizeBox = False
        form.MinimizeBox = False
        form.ClientSize = New Size(width, height)
    End Sub

End Module
