Imports System.Drawing
Imports Hrms.BLL
Imports System.Windows.Forms

''' <summary>
''' Base of every HRMS form: theme, hotkeys (F2, F3, F5, Ctrl+S, Ctrl+P, Ctrl+E, Esc) and Enter-to-next-field.
''' Child forms override the On* methods they need.
''' </summary>
Public Class BaseForm
    Inherits Form

    Protected Overridable Sub SelectCompanyRequested()
    End Sub

    Protected Overridable Sub FindRequested()
    End Sub

    Protected Overridable Sub RefreshRequested()
    End Sub

    Protected Overridable Sub SaveRequested()
    End Sub

    Protected Overridable Sub PrintRequested()
    End Sub

    Protected Overridable Sub ExportRequested()
    End Sub

    Protected Overridable Sub CancelRequested()
        Close()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        If Not DesignMode Then AppTheme.Apply(Me)
        MyBase.OnLoad(e)
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        Select Case keyData
            Case Keys.F2 : SelectCompanyRequested() : Return True
            Case Keys.F3 : FindRequested() : Return True
            Case Keys.F5 : RefreshRequested() : Return True
            Case Keys.Control Or Keys.S : SaveRequested() : Return True
            Case Keys.Control Or Keys.P : PrintRequested() : Return True
            Case Keys.Control Or Keys.E : ExportRequested() : Return True
            Case Keys.Escape
                If CancelButton Is Nothing Then CancelRequested() : Return True
            Case Keys.Enter
                If MoveToNextField() Then Return True
        End Select
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    ''' <summary>Enter moves to the next input. On the last input it falls through to the AcceptButton.</summary>
    Private Function MoveToNextField() As Boolean
        Dim current As Control = ActiveControl
        If current Is Nothing Then Return False
        Dim isInput As Boolean = TypeOf current Is ComboBox OrElse TypeOf current Is DateTimePicker OrElse
                                 TypeOf current Is NumericUpDown OrElse
                                 (TypeOf current Is TextBoxBase AndAlso Not DirectCast(current, TextBoxBase).Multiline)
        If Not isInput Then Return False

        Dim [next] As Control = GetNextControl(current, True)
        Do While [next] IsNot Nothing AndAlso Not ([next].TabStop AndAlso [next].Enabled AndAlso [next].Visible)
            [next] = GetNextControl([next], True)
        Loop
        If [next] Is Nothing Then Return False
        If TypeOf [next] Is Button AndAlso AcceptButton IsNot Nothing Then Return False

        [next].Select()
        Return True
    End Function

    ''' <summary>Adds a red asterisk after each label (mandatory field marker).</summary>
    Protected Sub MarkMandatory(ParamArray labels() As Label)
        For Each lbl As Label In labels
            Dim star As New Label With {
                .Text = "*",
                .AutoSize = True,
                .ForeColor = AppTheme.Danger,
                .BackColor = Color.Transparent,
                .Location = New Point(lbl.Right, lbl.Top)
            }
            lbl.Parent.Controls.Add(star)
            star.BringToFront()
        Next
    End Sub

    ''' <summary>
    ''' Runs a UI action. Unexpected failures are logged and shown as a short message with a log ID
    ''' (no stack trace); nothing is swallowed. Returns True when the action completed.
    ''' </summary>
    Protected Function RunGuarded(action As Action) As Boolean
        Dim previous As Cursor = Cursor
        Try
            Cursor = Cursors.WaitCursor
            action()
            Return True
        Catch ex As Exception
            Dim logId As String = AppServices.Logger.LogError(ex)
            Msg.ShowError(Me, "Something went wrong. Please try again or contact support.", logId)
            Return False
        Finally
            Cursor = previous
        End Try
    End Function

End Class
