Imports System.Linq
Imports Hrms.Migrator
Imports Xunit

Public Class MigrationPlannerTests
    Private Shared Function Script(version As Integer, Optional checksum As String = "") As MigrationScript
        Dim sum As String = If(checksum.Length > 0, checksum, "sum" & version)
        Return New MigrationScript With {.Version = version, .FileName = $"{version:0000}_x.sql", .Checksum = sum}
    End Function

    Private Shared Function Applied(version As Integer, Optional checksum As String = "") As AppliedMigration
        Dim sum As String = If(checksum.Length > 0, checksum, "sum" & version)
        Return New AppliedMigration With {.Version = version, .FileName = $"{version:0000}_x.sql", .Checksum = sum}
    End Function

    <Fact>
    Public Sub EverythingIsPendingOnAFirstRun()
        Dim plan As MigrationPlan = MigrationPlanner.Plan({Script(1), Script(2)}, New List(Of AppliedMigration))
        Assert.Equal(2, plan.Pending.Count)
        Assert.True(plan.IsFirstRun)
    End Sub

    <Fact>
    Public Sub AppliedScriptsAreSkipped()
        Dim plan As MigrationPlan = MigrationPlanner.Plan({Script(1), Script(2), Script(3)}, {Applied(1), Applied(2)})
        Assert.Equal(3, plan.Pending.Single().Version)
        Assert.False(plan.IsFirstRun)
    End Sub

    <Fact>
    Public Sub NothingPendingWhenAllApplied()
        Dim plan As MigrationPlan = MigrationPlanner.Plan({Script(1), Script(2)}, {Applied(1), Applied(2)})
        Assert.Empty(plan.Pending)
    End Sub

    <Fact>
    Public Sub EditedScriptIsRefused()
        Dim ex As MigrationException = Assert.Throws(Of MigrationException)(
            Function() MigrationPlanner.Plan({Script(1, "changed")}, {Applied(1, "original")}))
        Assert.Contains("edited after it was applied", ex.Message)
    End Sub

    <Fact>
    Public Sub MissingAppliedScriptIsRefused()
        Assert.Throws(Of MigrationException)(
            Function() MigrationPlanner.Plan({Script(2)}, {Applied(1), Applied(2)}))
    End Sub

    <Fact>
    Public Sub NewScriptBelowTheLatestAppliedIsRefused()
        Dim ex As MigrationException = Assert.Throws(Of MigrationException)(
            Function() MigrationPlanner.Plan({Script(1), Script(2), Script(3)}, {Applied(1), Applied(3)}))
        Assert.Contains("Renumber", ex.Message)
    End Sub

    <Fact>
    Public Sub GapsInNumberingAreAllowed()
        Dim plan As MigrationPlan = MigrationPlanner.Plan({Script(1), Script(10), Script(11)}, {Applied(1)})
        Assert.Equal(New List(Of Integer) From {10, 11}, plan.Pending.Select(Function(s) s.Version).ToList())
    End Sub
End Class
