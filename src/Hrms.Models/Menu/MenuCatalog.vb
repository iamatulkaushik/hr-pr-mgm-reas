Imports System.Linq

''' <summary>
''' All menu items of the 15-10-2026 release and the permission each one needs (docs/DESIGN.md s2, docs/ACCESS_CONTROL.md).
''' ForSession keeps only what the user may open and drops menus that end up empty.
''' Add an entry here when a module ships. Statutory, Career and Reports have no items in this release.
''' </summary>
Public NotInheritable Class MenuCatalog

    Private Shared ReadOnly _groups As IReadOnlyList(Of MenuGroup) = BuildGroups()

    Private Sub New()
    End Sub

    Public Shared Function ForSession(session As UserSession) As IReadOnlyList(Of MenuGroup)
        Dim result As New List(Of MenuGroup)()
        For Each menuGroup In _groups
            Dim visible = menuGroup.Entries.Where(Function(entry) session.CanAccess(entry.ModuleName, entry.Action)).ToList()
            If visible.Count > 0 Then
                result.Add(New MenuGroup(menuGroup.Title, visible))
            End If
        Next
        Return result
    End Function

    Private Shared Function BuildGroups() As IReadOnlyList(Of MenuGroup)
        Dim masters As New List(Of MenuEntry) From {
            New MenuEntry("masters.company", "Company", PermissionModules.COMPANY, PermissionActions.VIEW),
            New MenuEntry("masters.financialyear", "Financial year", PermissionModules.COMPANY, PermissionActions.EDIT),
            New MenuEntry("masters.department", "Department", PermissionModules.EMPLOYEE, PermissionActions.VIEW),
            New MenuEntry("masters.designation", "Designation", PermissionModules.EMPLOYEE, PermissionActions.VIEW),
            New MenuEntry("masters.employee", "Employee", PermissionModules.EMPLOYEE, PermissionActions.VIEW),
            New MenuEntry("masters.holidays", "Holiday calendar", PermissionModules.ATTENDANCE, PermissionActions.VIEW)
        }

        ' Month close locks and reopens attendance, so it needs the Unlock permission (Associate and Company only).
        Dim attendance As New List(Of MenuEntry) From {
            New MenuEntry("attendance.daily", "Daily attendance", PermissionModules.ATTENDANCE, PermissionActions.VIEW),
            New MenuEntry("attendance.monthly", "Monthly attendance", PermissionModules.ATTENDANCE, PermissionActions.VIEW),
            New MenuEntry("attendance.overtime", "Overtime", PermissionModules.ENTRY, PermissionActions.VIEW),
            New MenuEntry("attendance.monthclose", "Month close", PermissionModules.ATTENDANCE, PermissionActions.UNLOCK)
        }

        Dim payroll As New List(Of MenuEntry) From {
            New MenuEntry("payroll.structure", "Salary structure", PermissionModules.PAYROLL, PermissionActions.VIEW),
            New MenuEntry("payroll.advance", "Advance and loan", PermissionModules.ENTRY, PermissionActions.VIEW),
            New MenuEntry("payroll.arrears", "Arrears and bonus", PermissionModules.ENTRY, PermissionActions.VIEW),
            New MenuEntry("payroll.process", "Payroll process", PermissionModules.PAYROLL, PermissionActions.PROCESS),
            New MenuEntry("payroll.approve", "Payroll approve", PermissionModules.PAYROLL, PermissionActions.APPROVE)
        }

        Dim admin As New List(Of MenuEntry) From {
            New MenuEntry("admin.users", "Users", PermissionModules.USERS, PermissionActions.VIEW)
        }

        Return New List(Of MenuGroup) From {
            New MenuGroup("Masters", masters),
            New MenuGroup("Attendance", attendance),
            New MenuGroup("Payroll", payroll),
            New MenuGroup("Admin", admin)
        }
    End Function

End Class
