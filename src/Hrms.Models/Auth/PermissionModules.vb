''' <summary>
''' Module names used in permission codes ("Module.Action", for example Attendance.Edit).
''' Must match Database\Seed\Reference\001_roles_permissions.sql.
''' </summary>
Public NotInheritable Class PermissionModules

    Private Sub New()
    End Sub

    Public Const COMPANY As String = "Company"
    Public Const EMPLOYEE As String = "Employee"
    Public Const ATTENDANCE As String = "Attendance"
    Public Const ENTRY As String = "Entry"
    Public Const PAYROLL As String = "Payroll"
    Public Const PAYSLIP As String = "Payslip"
    Public Const STATUTORY As String = "Statutory"
    Public Const RATES As String = "Rates"
    Public Const CAREER As String = "Career"
    Public Const REPORTS As String = "Reports"
    Public Const USERS As String = "Users"
    Public Const BACKUP As String = "Backup"
    Public Const AUDIT As String = "Audit"

End Class
