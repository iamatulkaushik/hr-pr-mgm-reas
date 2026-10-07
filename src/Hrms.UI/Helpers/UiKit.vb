Imports System.Drawing
Imports System.Windows.Forms

' Shared look and message helpers for the Phase 2 screens. Colours and sizes follow DESIGN.md.
Public NotInheritable Class UiKit

    Public Const DATE_FORMAT As String = "dd-MM-yyyy"
    Public Const MONTH_FORMAT As String = "MM-yyyy"
    Public Const TIME_FORMAT As String = "hh\:mm"
    Public Const MESSAGE_CAPTION As String = "Revolution HRMS"
    Public Const BUTTON_WIDTH As Integer = 96
    Public Const BUTTON_HEIGHT As Integer = 30
    Public Const BAR_HEIGHT As Integer = 44
    Public Const FOOTER_HEIGHT As Integer = 52

    Private Const STATUS_DURATION_MS As Integer = 3000
    Private Const STATUS_LABEL_NAME As String = "tslMessage"
    Private Const HOURS_MAX As Integer = 23
    Private Const MINUTES_MAX As Integer = 59
    Private Const TIME_DIGITS As Integer = 4

    Public Shared ReadOnly Primary As Color = ColorTranslator.FromHtml("#1F4E79")
    Public Shared ReadOnly Accent As Color = ColorTranslator.FromHtml("#2E8B57")
    Public Shared ReadOnly Danger As Color = ColorTranslator.FromHtml("#B22222")
    Public Shared ReadOnly Background As Color = ColorTranslator.FromHtml("#F4F6F8")
    Public Shared ReadOnly GridAlt As Color = ColorTranslator.FromHtml("#EEF3F8")
    Public Shared ReadOnly LockedBack As Color = Color.FromArgb(235, 235, 235)

    Private Sub New()
    End Sub

    Public Shared Function CreateButton(text As String, kind As ButtonKind) As Button
        Dim button As New Button() With {
            .Text = text,
            .Width = BUTTON_WIDTH,
            .Height = BUTTON_HEIGHT,
            .FlatStyle = FlatStyle.Flat,
            .UseVisualStyleBackColor = False
        }
        Select Case kind
            Case ButtonKind.Primary
                button.BackColor = Accent
                button.ForeColor = Color.White
            Case ButtonKind.Danger
                button.BackColor = Danger
                button.ForeColor = Color.White
            Case Else
                button.BackColor = Color.White
                button.ForeColor = Primary
        End Select
        button.FlatAppearance.BorderColor = Primary
        Return button
    End Function

    ' A label followed by a red asterisk, for mandatory fields.
    Public Shared Function RequiredLabel(text As String) As Control
        Dim panel As New FlowLayoutPanel() With {.AutoSize = True, .WrapContents = False, .Margin = New Padding(0)}
        panel.Controls.Add(New Label() With {.Text = text, .AutoSize = True, .Margin = New Padding(0, 6, 0, 0)})
        panel.Controls.Add(New Label() With {.Text = "*", .AutoSize = True, .ForeColor = Danger, .Margin = New Padding(2, 6, 0, 0)})
        Return panel
    End Function

    Public Shared Sub StyleGrid(grid As DataGridView, rowSelect As Boolean)
        grid.BackgroundColor = Color.White
        grid.BorderStyle = BorderStyle.None
        grid.EnableHeadersVisualStyles = False
        grid.ColumnHeadersDefaultCellStyle.BackColor = Primary
        grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primary
        grid.AlternatingRowsDefaultCellStyle.BackColor = GridAlt
        grid.RowHeadersVisible = False
        grid.AllowUserToAddRows = False
        grid.AllowUserToDeleteRows = False
        grid.AllowUserToResizeRows = False
        grid.MultiSelect = False
        grid.SelectionMode = If(rowSelect, DataGridViewSelectionMode.FullRowSelect, DataGridViewSelectionMode.CellSelect)
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
    End Sub

    Public Shared Sub AddTextColumn(grid As DataGridView, name As String, header As String, fillWeight As Single,
                                    isReadOnly As Boolean, rightAlign As Boolean)
        Dim column As New DataGridViewTextBoxColumn() With {
            .Name = name,
            .HeaderText = header,
            .FillWeight = fillWeight,
            .ReadOnly = isReadOnly,
            .SortMode = DataGridViewColumnSortMode.NotSortable
        }
        If rightAlign Then
            column.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight
        End If
        grid.Columns.Add(column)
    End Sub

    Public Shared Sub ShowInfo(owner As IWin32Window, text As String)
        MessageBox.Show(owner, text, MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Public Shared Sub ShowWarning(owner As IWin32Window, text As String)
        MessageBox.Show(owner, text, MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Public Shared Sub ShowError(owner As IWin32Window, text As String)
        MessageBox.Show(owner, text, MESSAGE_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    ' Amber warning that asks the user to confirm. The default button is No.
    Public Shared Function Confirm(owner As IWin32Window, text As String) As Boolean
        Dim answer As DialogResult = MessageBox.Show(owner, text, MESSAGE_CAPTION, MessageBoxButtons.YesNo,
                                                     MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2)
        Return answer = DialogResult.Yes
    End Function

    ' Success text shows for 3 seconds on the form's own status label and on the main status bar when it has one.
    Public Shared Sub ShowSuccess(owner As Form, text As String, localLabel As Label)
        Dim targets As New List(Of Control)()
        If localLabel IsNot Nothing Then
            localLabel.ForeColor = Accent
            localLabel.Text = text
            targets.Add(localLabel)
        End If
        Dim statusItem As ToolStripItem = FindMainStatusItem(owner)
        If statusItem IsNot Nothing Then
            statusItem.Text = text
        End If
        Dim timer As New System.Windows.Forms.Timer() With {.Interval = STATUS_DURATION_MS}
        AddHandler timer.Tick,
            Sub(sender As Object, e As EventArgs)
                timer.Stop()
                timer.Dispose()
                If localLabel IsNot Nothing AndAlso Not localLabel.IsDisposed AndAlso localLabel.Text = text Then
                    localLabel.Text = String.Empty
                End If
                If statusItem IsNot Nothing AndAlso statusItem.Text = text Then
                    statusItem.Text = String.Empty
                End If
            End Sub
        timer.Start()
    End Sub

    Public Shared Sub ShowProblem(localLabel As Label, text As String)
        localLabel.ForeColor = Danger
        localLabel.Text = text
    End Sub

    Public Shared Function TryParseTime(text As String, ByRef result As TimeSpan) As Boolean
        Dim value As String = If(text, String.Empty).Trim()
        Dim hours As Integer
        Dim minutes As Integer
        If value.Length = 0 Then
            Return False
        End If
        If value.Contains(":") Then
            Dim parts As String() = value.Split(":"c)
            If parts.Length <> 2 OrElse Not Integer.TryParse(parts(0), hours) OrElse Not Integer.TryParse(parts(1), minutes) Then
                Return False
            End If
        Else
            If value.Length > TIME_DIGITS OrElse Not value.All(Function(c) Char.IsDigit(c)) Then
                Return False
            End If
            Dim padded As String = value.PadLeft(TIME_DIGITS, "0"c)
            hours = Integer.Parse(padded.Substring(0, 2))
            minutes = Integer.Parse(padded.Substring(2, 2))
        End If
        If hours < 0 OrElse hours > HOURS_MAX OrElse minutes < 0 OrElse minutes > MINUTES_MAX Then
            Return False
        End If
        result = New TimeSpan(hours, minutes, 0)
        Return True
    End Function

    Public Shared Function FormatTime(value As TimeSpan?) As String
        If Not value.HasValue Then
            Return String.Empty
        End If
        Return value.Value.ToString(TIME_FORMAT)
    End Function

    Private Shared Function FindMainStatusItem(owner As Form) As ToolStripItem
        If owner Is Nothing OrElse owner.MdiParent Is Nothing Then
            Return Nothing
        End If
        For Each control As Control In owner.MdiParent.Controls
            Dim strip As StatusStrip = TryCast(control, StatusStrip)
            If strip IsNot Nothing AndAlso strip.Items.ContainsKey(STATUS_LABEL_NAME) Then
                Return strip.Items(STATUS_LABEL_NAME)
            End If
        Next
        Return Nothing
    End Function

End Class
