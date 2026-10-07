Imports System.Windows.Forms
Imports Hrms.Models

''' <summary>Builds the module menus from MenuCatalog for the signed-in user.</summary>
Friend Module MenuBuilder

    ''' <summary>Adds one top-level menu per group the user may see. onOpen receives the menu key.</summary>
    Public Sub AddModuleMenus(menu As MenuStrip, session As UserSession, registry As FormRegistry, onOpen As Action(Of String))
        menu.ShowItemToolTips = True
        For Each menuGroup In MenuCatalog.ForSession(session)
            Dim top As New ToolStripMenuItem(menuGroup.Title)
            For Each entry In menuGroup.Entries
                Dim key = entry.Key
                Dim item As New ToolStripMenuItem(entry.Title) With {.Enabled = registry.IsRegistered(key)}
                If Not item.Enabled Then
                    item.ToolTipText = "This screen is not available yet."
                End If
                AddHandler item.Click, Sub(sender, e) onOpen(key)
                top.DropDownItems.Add(item)
            Next
            menu.Items.Add(top)
        Next
    End Sub

End Module
