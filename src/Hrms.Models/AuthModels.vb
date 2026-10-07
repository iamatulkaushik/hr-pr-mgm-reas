Option Strict On

Public Enum UserType
    Associate = 1
    Company = 2
    [Operator] = 3
End Enum

Public Class CompanyInfo
    Public Property CompanyID As Integer
    Public Property CompanyCode As String = ""
    Public Property CompanyName As String = ""
    Public Overrides Function ToString() As String
        Return CompanyName
    End Function
End Class

Public Class SessionInfo
    Public Property UserID As Integer
    Public Property Username As String = ""
    Public Property FullName As String = ""
    Public Property RoleID As Integer
    Public Property UserType As UserType
    Public Property MachineName As String = Environment.MachineName
    Public Property MustChangePassword As Boolean
    Public Property LoginTimeUtc As DateTime = DateTime.UtcNow
    Public Property AllowedCompanies As New List(Of CompanyInfo)
    Public Property CurrentCompany As CompanyInfo
    Public ReadOnly Property Permissions As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
End Class

Public Class AuthResult
    Public Property Success As Boolean
    Public Property Message As String = ""
    Public Property Session As SessionInfo
    Public Property LockedUntilUtc As DateTime?
End Class

Public Class OperationResult
    Public Property Success As Boolean
    Public Property Message As String = ""
    Public Shared Function Ok(Optional message As String = "") As OperationResult
        Return New OperationResult With {.Success = True, .Message = message}
    End Function
    Public Shared Function Fail(message As String) As OperationResult
        Return New OperationResult With {.Success = False, .Message = message}
    End Function
End Class

''' <summary>Permission codes. Must match the sec.Permission seed (Module.Action).</summary>
Public Module Perm
    Public Const CompanyView As String = "Company.View"
    Public Const CompanyAdd As String = "Company.Add"
    Public Const CompanyEdit As String = "Company.Edit"
    Public Const CompanyDeactivate As String = "Company.Deactivate"
    Public Const EmployeeView As String = "Employee.View"
    Public Const EmployeeAdd As String = "Employee.Add"
    Public Const EmployeeEdit As String = "Employee.Edit"
    Public Const EmployeeDeactivate As String = "Employee.Deactivate"
    Public Const UserView As String = "User.View"
    Public Const UserAdd As String = "User.Add"
    Public Const UserEdit As String = "User.Edit"
    Public Const UserDeactivate As String = "User.Deactivate"
    Public Const UserAssignCompany As String = "User.AssignCompany"
    Public Const AttendanceView As String = "Attendance.View"
    Public Const AttendanceAdd As String = "Attendance.Add"
    Public Const AttendanceEdit As String = "Attendance.Edit"
    Public Const AttendanceLock As String = "Attendance.Lock"
    Public Const AttendanceReopen As String = "Attendance.Reopen"
    Public Const PayrollView As String = "Payroll.View"
    Public Const PayrollProcess As String = "Payroll.Process"
    Public Const PayrollApprove As String = "Payroll.Approve"
    Public Const PayrollLock As String = "Payroll.Lock"
    Public Const StatutoryView As String = "Statutory.View"
    Public Const StatutoryEdit As String = "Statutory.Edit"
    Public Const CareerView As String = "Career.View"
    Public Const CareerAdd As String = "Career.Add"
    Public Const ReportsView As String = "Reports.View"
    Public Const ReportsExport As String = "Reports.Export"
    Public Const AuditView As String = "Audit.View"
    Public Const AdminBackup As String = "Admin.Backup"
    Public Const AdminRestore As String = "Admin.Restore"
End Module
