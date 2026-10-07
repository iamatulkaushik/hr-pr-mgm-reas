Imports Revolution.Hrms.Models

''' <summary>
''' Sessions with the permission list of each user type.
''' Mirror of Database\Seed\Reference\001_roles_permissions.sql: change both together.
''' </summary>
Friend Module TestSessions

    Private ReadOnly _associateCodes As String() = {
        "Company.View", "Company.Edit", "Employee.View", "Employee.Edit", "Attendance.View", "Attendance.Edit",
        "Attendance.Unlock", "Entry.View", "Entry.Edit", "Payroll.View", "Payroll.Process", "Payroll.Approve",
        "Payslip.View", "Statutory.View", "Statutory.Edit", "Rates.Edit", "Career.View", "Career.Edit",
        "Reports.View", "Reports.Payroll", "Users.View", "Users.Edit", "Backup.Run", "Audit.View"}

    Private ReadOnly _companyCodes As String() = {
        "Company.View", "Employee.View", "Employee.Edit", "Attendance.View", "Attendance.Edit", "Attendance.Unlock",
        "Entry.View", "Entry.Edit", "Payroll.View", "Payroll.Process", "Payroll.Approve", "Payslip.View",
        "Statutory.View", "Statutory.Edit", "Career.View", "Career.Edit", "Reports.View", "Reports.Payroll",
        "Users.View", "Users.Edit", "Audit.View"}

    Private ReadOnly _operatorCodes As String() = {
        "Employee.View", "Employee.Edit", "Attendance.View", "Attendance.Edit", "Entry.View", "Entry.Edit",
        "Payslip.View", "Reports.View"}

    Public Function Create(kind As UserType, ParamArray companyIds As Integer()) As UserSession
        Dim companies = companyIds.Select(Function(id) New CompanyInfo(id, $"Company {id}")).ToList()
        Return New UserSession(1, "tester", kind, CodesFor(kind), companies, False)
    End Function

    Public Function WithoutPermissions() As UserSession
        Return New UserSession(2, "nobody", UserType.[Operator], New String() {}, New List(Of CompanyInfo)(), False)
    End Function

    Private Function CodesFor(kind As UserType) As String()
        Select Case kind
            Case UserType.Associate
                Return _associateCodes
            Case UserType.Company
                Return _companyCodes
            Case Else
                Return _operatorCodes
        End Select
    End Function

End Module
