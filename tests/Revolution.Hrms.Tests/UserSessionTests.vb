Imports Revolution.Hrms.Models
Imports Xunit

Public Class UserSessionTests

    <Theory>
    <InlineData(UserType.Associate, "Rates", "Edit", True)>
    <InlineData(UserType.Company, "Rates", "Edit", False)>
    <InlineData(UserType.[Operator], "Rates", "Edit", False)>
    <InlineData(UserType.[Operator], "Attendance", "Edit", True)>
    <InlineData(UserType.[Operator], "Attendance", "Unlock", False)>
    <InlineData(UserType.Company, "Attendance", "Unlock", True)>
    <InlineData(UserType.[Operator], "Employee", "Edit", True)>
    <InlineData(UserType.[Operator], "Entry", "Edit", True)>
    <InlineData(UserType.[Operator], "Payslip", "View", True)>
    <InlineData(UserType.[Operator], "Payroll", "Process", False)>
    <InlineData(UserType.[Operator], "Payroll", "Approve", False)>
    <InlineData(UserType.[Operator], "Statutory", "Edit", False)>
    <InlineData(UserType.[Operator], "Career", "Edit", False)>
    <InlineData(UserType.[Operator], "Users", "View", False)>
    <InlineData(UserType.[Operator], "Backup", "Run", False)>
    <InlineData(UserType.[Operator], "Audit", "View", False)>
    <InlineData(UserType.Company, "Payroll", "Approve", True)>
    <InlineData(UserType.Company, "Company", "Edit", False)>
    <InlineData(UserType.Company, "Backup", "Run", False)>
    <InlineData(UserType.Associate, "Backup", "Run", True)>
    Public Sub CanAccess_FollowsThePermissionMatrix(kind As UserType, moduleName As String, action As String, expected As Boolean)
        Dim session = TestSessions.Create(kind, 1)
        Assert.Equal(expected, session.CanAccess(moduleName, action))
    End Sub

    <Fact>
    Public Sub CanAccess_IgnoresCase()
        Dim session = TestSessions.Create(UserType.[Operator], 1)
        Assert.True(session.CanAccess("attendance", "EDIT"))
    End Sub

    <Fact>
    Public Sub CanAccess_UnknownPermission_IsFalse()
        Dim session = TestSessions.Create(UserType.Associate, 1)
        Assert.False(session.CanAccess("Payroll", "Delete"))
    End Sub

    <Fact>
    Public Sub Require_WithoutPermission_Throws()
        Dim session = TestSessions.Create(UserType.[Operator], 1)
        Assert.Throws(Of AccessDeniedException)(Sub() session.Require("Payroll", "Approve"))
    End Sub

    <Fact>
    Public Sub Require_WithPermission_DoesNotThrow()
        Dim session = TestSessions.Create(UserType.[Operator], 1)
        session.Require("Attendance", "Edit")
    End Sub

    <Fact>
    Public Sub CanUseCompany_OnlyForAssignedCompanies()
        Dim session = TestSessions.Create(UserType.[Operator], 10, 20)
        Assert.True(session.CanUseCompany(10))
        Assert.True(session.CanUseCompany(20))
        Assert.False(session.CanUseCompany(30))
    End Sub

    <Fact>
    Public Sub RequireCompany_UnassignedCompany_Throws()
        Dim session = TestSessions.Create(UserType.[Operator], 10)
        Assert.Throws(Of AccessDeniedException)(Sub() session.RequireCompany(99))
    End Sub

    <Fact>
    Public Sub ReplaceCompanies_UpdatesTheList()
        Dim session = TestSessions.Create(UserType.Associate, 1)
        session.ReplaceCompanies({New CompanyInfo(1, "One"), New CompanyInfo(2, "Two")})
        Assert.Equal(2, session.Companies.Count)
        Assert.True(session.CanUseCompany(2))
    End Sub

    <Fact>
    Public Sub NoPermissions_CannotDoAnything()
        Dim session = TestSessions.WithoutPermissions()
        Assert.False(session.CanAccess("Employee", "View"))
        Assert.False(session.CanUseCompany(1))
    End Sub

End Class
