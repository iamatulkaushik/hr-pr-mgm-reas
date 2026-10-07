''' <summary>One menu item: a key the UI maps to a form, plus the permission needed to see it.</summary>
Public NotInheritable Class MenuEntry

    Public Sub New(key As String, title As String, moduleName As String, action As String)
        Me.Key = key
        Me.Title = title
        Me.ModuleName = moduleName
        Me.Action = action
    End Sub

    Public ReadOnly Property Key As String
    Public ReadOnly Property Title As String
    Public ReadOnly Property ModuleName As String
    Public ReadOnly Property Action As String

End Class
