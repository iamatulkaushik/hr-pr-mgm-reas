Imports Revolution.Hrms.Models
Imports Xunit

Public Class MenuCatalogTests

    Private Shared Function KeysOf(session As UserSession) As List(Of String)
        Return MenuCatalog.ForSession(session).SelectMany(Function(g) g.Entries).Select(Function(e) e.Key).ToList()
    End Function

    Private Shared Function Titles(session As UserSession) As List(Of String)
        Return MenuCatalog.ForSession(session).Select(Function(g) g.Title).ToList()
    End Function

    <Fact>
    Public Sub Associate_SeesEveryMenu()
        Dim session = TestSessions.Create(UserType.Associate, 1)
        Assert.Equal(New List(Of String) From {"Masters", "Attendance", "Payroll", "Admin"}, Titles(session))
    End Sub

    <Fact>
    Public Sub Operator_SeesEntryScreensButNotAdminOrPayrollProcessing()
        Dim session = TestSessions.Create(UserType.[Operator], 1)
        Dim keys = KeysOf(session)

        Assert.DoesNotContain("Admin", Titles(session))
        Assert.Contains("masters.employee", keys)
        Assert.Contains("attendance.daily", keys)
        Assert.Contains("attendance.monthly", keys)
        Assert.Contains("payroll.advance", keys)
        Assert.Contains("payroll.arrears", keys)

        Assert.DoesNotContain("payroll.process", keys)
        Assert.DoesNotContain("payroll.approve", keys)
        Assert.DoesNotContain("payroll.structure", keys)
        Assert.DoesNotContain("admin.users", keys)
        Assert.DoesNotContain("masters.company", keys)
        Assert.DoesNotContain("attendance.monthclose", keys)
    End Sub

    <Fact>
    Public Sub CompanyUser_CanProcessPayrollAndCloseTheMonth_ButNotEditFinancialYear()
        Dim session = TestSessions.Create(UserType.Company, 1)
        Dim keys = KeysOf(session)

        Assert.Contains("payroll.process", keys)
        Assert.Contains("payroll.approve", keys)
        Assert.Contains("attendance.monthclose", keys)
        Assert.Contains("admin.users", keys)
        Assert.DoesNotContain("masters.financialyear", keys)
    End Sub

    <Fact>
    Public Sub MenusWithNoVisibleItems_AreLeftOut()
        Dim session = TestSessions.WithoutPermissions()
        Assert.Empty(MenuCatalog.ForSession(session))
    End Sub

    <Fact>
    Public Sub EveryKey_IsUnique()
        Dim session = TestSessions.Create(UserType.Associate, 1)
        Dim keys = KeysOf(session)
        Assert.Equal(keys.Count, keys.Distinct(StringComparer.OrdinalIgnoreCase).Count())
    End Sub

End Class
