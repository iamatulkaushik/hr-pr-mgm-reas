Imports System.Windows.Forms

Public NotInheritable Class Hotkeys

    Private Sub New()
    End Sub

    Public Shared Function Resolve(keyData As Keys) As HotkeyAction
        Select Case keyData
            Case Keys.F2
                Return HotkeyAction.SelectCompany
            Case Keys.F3
                Return HotkeyAction.Find
            Case Keys.F5
                Return HotkeyAction.Refresh
            Case Keys.Control Or Keys.S
                Return HotkeyAction.Save
            Case Keys.Control Or Keys.P
                Return HotkeyAction.Print
            Case Keys.Control Or Keys.E
                Return HotkeyAction.Export
            Case Keys.Escape
                Return HotkeyAction.CloseForm
            Case Keys.Enter
                Return HotkeyAction.NextField
            Case Else
                Return HotkeyAction.None
        End Select
    End Function

End Class
