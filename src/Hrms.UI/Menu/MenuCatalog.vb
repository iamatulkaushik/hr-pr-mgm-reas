Imports Hrms.Models.Auth

''' <summary>
''' Menu of the main window (DESIGN.md: Masters, Attendance, Payroll, Statutory, Career, Reports, Admin).
''' Only screens of the 15-10 release are listed; Statutory, Career and Reports stay empty until Later,
''' so their top menus are hidden. Visibility = BLL permission check; the BLL enforces again on every call.
''' </summary>
Public NotInheritable Class MenuCatalog

    Private Shared ReadOnly _topLevel As String() = {"Masters", "Attendance", "Payroll", "Statutory", "Career", "Reports", "Admin"}

    Private Shared ReadOnly _items As IReadOnlyList(Of MenuItemDef) = BuildItems()

    Private Sub New()
    End Sub

    Private Shared Function BuildItems() As IReadOnlyList(Of MenuItemDef)
        Return New List(Of MenuItemDef) From {
            New MenuItemDef("Company", "Masters", "Company", Perm.MOD_COMPANY, Perm.ACT_VIEW, "frmCompany"),
            New MenuItemDef("Department", "Masters", "Department", Perm.MOD_MASTER, Perm.ACT_OPERATE, "frmDepartment"),
            New MenuItemDef("Designation", "Masters", "Designation", Perm.MOD_MASTER, Perm.ACT_OPERATE, "frmDesignation"),
            New MenuItemDef("Employee", "Masters", "Employee", Perm.MOD_EMPLOYEE, Perm.ACT_VIEW, "frmEmployee"),
            New MenuItemDef("HolidayCalendar", "Masters", "Holiday calendar", Perm.MOD_MASTER, Perm.ACT_OPERATE, "frmHolidayCalendar"),
            New MenuItemDef("FinancialYear", "Masters", "Financial year", Perm.MOD_MASTER, Perm.ACT_OPERATE, "frmFinancialYear"),
            New MenuItemDef("DailyAttendance", "Attendance", "Daily attendance", Perm.MOD_ATTENDANCE, Perm.ACT_OPERATE, "frmDailyAttendance"),
            New MenuItemDef("MonthlyAttendance", "Attendance", "Monthly summary", Perm.MOD_ATTENDANCE, Perm.ACT_OPERATE, "frmMonthlyAttendance"),
            New MenuItemDef("Overtime", "Attendance", "Overtime", Perm.MOD_OVERTIME, Perm.ACT_OPERATE, "frmOvertime"),
            New MenuItemDef("MonthClose", "Attendance", "Month close", Perm.MOD_ATTENDANCE, Perm.ACT_CLOSE, "frmMonthClose"),
            New MenuItemDef("SalaryStructure", "Payroll", "Salary structure", Perm.MOD_PAYROLL, Perm.ACT_PROCESS, "frmSalaryStructure"),
            New MenuItemDef("AdvanceLoan", "Payroll", "Advance / loan", Perm.MOD_ADVANCE, Perm.ACT_OPERATE, "frmAdvanceLoan"),
            New MenuItemDef("ArrearsBonus", "Payroll", "Arrears / bonus", Perm.MOD_ARREARS, Perm.ACT_OPERATE, "frmArrearsBonus"),
            New MenuItemDef("PayrollProcess", "Payroll", "Process payroll", Perm.MOD_PAYROLL, Perm.ACT_PROCESS, "frmPayrollProcess"),
            New MenuItemDef("PayrollApprove", "Payroll", "Approve payroll", Perm.MOD_PAYROLL, Perm.ACT_APPROVE, "frmPayrollApprove"),
            New MenuItemDef("Users", "Admin", "Users", Perm.MOD_USERS, Perm.ACT_MANAGE, "frmUsers"),
            New MenuItemDef("ChangePassword", "Admin", "Change password", command:=MenuCommand.ChangePassword),
            New MenuItemDef("LockScreen", "Admin", "Lock screen", command:=MenuCommand.LockScreen, shortcutText:="Ctrl+L"),
            New MenuItemDef("LogOut", "Admin", "Log out", command:=MenuCommand.LogOut),
            New MenuItemDef("Exit", "Admin", "Exit", command:=MenuCommand.ExitApp)
        }
    End Function

    Public Shared Function TopLevelNames() As IReadOnlyList(Of String)
        Return _topLevel
    End Function

    ''' <summary>Top menus with only the entries the user may open. A top menu without entries is dropped.</summary>
    Public Shared Function GetVisible(canAccess As Func(Of String, String, Boolean)) As IReadOnlyList(Of MenuNode)
        Dim nodes As New List(Of MenuNode)
        For Each top As String In _topLevel
            Dim children As List(Of MenuItemDef) = _items.
                Where(Function(i) i.ParentKey = top AndAlso (i.IsAlwaysVisible OrElse canAccess(i.ModuleName, i.Action))).
                ToList()
            If children.Count > 0 Then nodes.Add(New MenuNode(New MenuItemDef(top, Nothing, top), children))
        Next
        Return nodes
    End Function

End Class
