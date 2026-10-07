''' <summary>A top-level menu (Masters, Attendance, ...) and its items.</summary>
Public NotInheritable Class MenuGroup

    Public Sub New(title As String, entries As IReadOnlyList(Of MenuEntry))
        Me.Title = title
        Me.Entries = entries
    End Sub

    Public ReadOnly Property Title As String
    Public ReadOnly Property Entries As IReadOnlyList(Of MenuEntry)

End Class
