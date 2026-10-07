Public Enum MenuCommand
    OpenForm = 0
    ChangePassword = 1
    LockScreen = 2
    LogOut = 3
    ExitApp = 4
End Enum

''' <summary>One menu entry. ModuleName/Action Nothing = always visible.</summary>
Public NotInheritable Class MenuItemDef

    Public ReadOnly Property Key As String
    Public ReadOnly Property ParentKey As String
    Public ReadOnly Property Text As String
    Public ReadOnly Property ModuleName As String
    Public ReadOnly Property Action As String
    Public ReadOnly Property FormName As String
    Public ReadOnly Property Command As MenuCommand
    Public ReadOnly Property ShortcutText As String

    Public Sub New(key As String, parentKey As String, text As String,
                   Optional moduleName As String = Nothing, Optional action As String = Nothing,
                   Optional formName As String = Nothing, Optional command As MenuCommand = MenuCommand.OpenForm,
                   Optional shortcutText As String = Nothing)
        Me.Key = key
        Me.ParentKey = parentKey
        Me.Text = text
        Me.ModuleName = moduleName
        Me.Action = action
        Me.FormName = formName
        Me.Command = command
        Me.ShortcutText = shortcutText
    End Sub

    Public ReadOnly Property IsAlwaysVisible As Boolean
        Get
            Return ModuleName Is Nothing
        End Get
    End Property

End Class

Public NotInheritable Class MenuNode
    Public ReadOnly Property Def As MenuItemDef
    Public ReadOnly Property Children As IReadOnlyList(Of MenuItemDef)

    Public Sub New(def As MenuItemDef, children As IReadOnlyList(Of MenuItemDef))
        Me.Def = def
        Me.Children = children
    End Sub
End Class
