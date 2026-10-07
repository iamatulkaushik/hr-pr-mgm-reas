Imports System.Windows.Forms
Imports Hrms.UI
Imports Xunit

Public Class HotkeysTests

    <Theory>
    <InlineData(Keys.F2, HotkeyAction.SelectCompany)>
    <InlineData(Keys.F3, HotkeyAction.Find)>
    <InlineData(Keys.F5, HotkeyAction.Refresh)>
    <InlineData(Keys.Control Or Keys.S, HotkeyAction.Save)>
    <InlineData(Keys.Control Or Keys.P, HotkeyAction.Print)>
    <InlineData(Keys.Control Or Keys.E, HotkeyAction.Export)>
    <InlineData(Keys.Escape, HotkeyAction.CloseForm)>
    <InlineData(Keys.Enter, HotkeyAction.NextField)>
    <InlineData(Keys.A, HotkeyAction.None)>
    <InlineData(Keys.S, HotkeyAction.None)>
    Public Sub Resolve_MapsKeyToAction(key As Keys, expected As HotkeyAction)
        Assert.Equal(expected, Hotkeys.Resolve(key))
    End Sub

End Class
