Imports System.Windows.Forms

''' <summary>Turns the visible menu tree into a MenuStrip. Clicks go to one callback; frmMain decides what to open.</summary>
Public NotInheritable Class MenuBuilder

    Private Sub New()
    End Sub

    Public Shared Sub Build(menu As MenuStrip, nodes As IReadOnlyList(Of MenuNode), onInvoke As Action(Of MenuItemDef))
        menu.Items.Clear()
        For Each node As MenuNode In nodes
            Dim top As New ToolStripMenuItem(node.Def.Text) With {.Name = "mnu" & node.Def.Key}
            For Each child As MenuItemDef In node.Children
                Dim def As MenuItemDef = child
                Dim item As New ToolStripMenuItem(def.Text) With {.Name = "mnu" & def.Key}
                If def.ShortcutText IsNot Nothing Then item.ShortcutKeyDisplayString = def.ShortcutText
                AddHandler item.Click, Sub() onInvoke(def)
                top.DropDownItems.Add(item)
            Next
            menu.Items.Add(top)
        Next
    End Sub

End Class
