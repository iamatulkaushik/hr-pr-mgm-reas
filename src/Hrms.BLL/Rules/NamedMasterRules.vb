Imports Hrms.Models

Public NotInheritable Class NamedMasterRules

    Public Const MAX_NAME_LENGTH As Integer = 100

    Private Sub New()
    End Sub

    Public Shared Function CleanName(name As String) As String
        If name Is Nothing Then
            Return String.Empty
        End If
        Return String.Join(" ", name.Split(New Char() {" "c}, StringSplitOptions.RemoveEmptyEntries))
    End Function

    Public Shared Sub EnsureValid(item As NamedMasterDto, existing As IEnumerable(Of NamedMasterDto), label As String)
        If item.Name.Length = 0 Then
            Throw New BusinessRuleException($"{label} name is required.")
        End If
        If item.Name.Length > MAX_NAME_LENGTH Then
            Throw New BusinessRuleException($"{label} name cannot be longer than {MAX_NAME_LENGTH} characters.")
        End If
        Dim clash As Boolean = existing.Any(Function(e) e.Id <> item.Id AndAlso String.Equals(e.Name, item.Name, StringComparison.OrdinalIgnoreCase))
        If clash Then
            Throw New BusinessRuleException($"{label} '{item.Name}' already exists.")
        End If
    End Sub

End Class
