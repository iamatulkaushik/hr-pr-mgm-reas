Imports System.Windows.Forms

''' <summary>
''' Maps a menu key (see MenuCatalog) to a factory that creates the form.
''' A module registers its forms in ModuleRegistrations when it ships. Menu items without a form stay disabled.
''' </summary>
Friend NotInheritable Class FormRegistry

    Private ReadOnly _factories As New Dictionary(Of String, Func(Of Form))(StringComparer.OrdinalIgnoreCase)

    Public Sub Register(key As String, factory As Func(Of Form))
        _factories(key) = factory
    End Sub

    Public Function IsRegistered(key As String) As Boolean
        Return _factories.ContainsKey(key)
    End Function

    Public Function Create(key As String) As Form
        Return _factories(key)()
    End Function

End Class
