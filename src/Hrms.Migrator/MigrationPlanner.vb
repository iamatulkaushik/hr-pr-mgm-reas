Imports System.Linq

''' <summary>
''' Decides what is pending. Rules:
'''   - an applied script must still exist with the same checksum (merged scripts are never edited);
'''   - a new script must be numbered above the latest applied one.
''' </summary>
Public NotInheritable Class MigrationPlanner
    Private Sub New()
    End Sub

    Public Shared Function Plan(scripts As IReadOnlyList(Of MigrationScript), applied As IReadOnlyList(Of AppliedMigration)) As MigrationPlan
        Dim byVersion As Dictionary(Of Integer, MigrationScript) = scripts.ToDictionary(Function(s) s.Version)
        For Each done As AppliedMigration In applied
            VerifyApplied(done, byVersion)
        Next

        Dim appliedVersions As HashSet(Of Integer) = New HashSet(Of Integer)(applied.Select(Function(a) a.Version))
        Dim pending As List(Of MigrationScript) = scripts.Where(Function(s) Not appliedVersions.Contains(s.Version)).ToList()
        EnsureOrder(pending, applied)
        Return New MigrationPlan(pending, applied.Count)
    End Function

    Private Shared Sub VerifyApplied(done As AppliedMigration, byVersion As Dictionary(Of Integer, MigrationScript))
        Dim script As MigrationScript = Nothing
        If Not byVersion.TryGetValue(done.Version, script) Then
            Throw New MigrationException($"Applied script {done.FileName} is missing from the scripts folder.")
        End If
        If Not String.Equals(script.Checksum, done.Checksum, StringComparison.OrdinalIgnoreCase) Then
            Throw New MigrationException($"{done.FileName} was edited after it was applied. Restore it and add a new numbered script instead.")
        End If
    End Sub

    Private Shared Sub EnsureOrder(pending As List(Of MigrationScript), applied As IReadOnlyList(Of AppliedMigration))
        If applied.Count = 0 OrElse pending.Count = 0 Then Return
        Dim latest As Integer = applied.Max(Function(a) a.Version)
        Dim outOfOrder As MigrationScript = pending.FirstOrDefault(Function(s) s.Version < latest)
        If outOfOrder IsNot Nothing Then
            Throw New MigrationException($"{outOfOrder.FileName} is older than the latest applied script ({latest:0000}). Renumber it above {latest:0000}.")
        End If
    End Sub
End Class
