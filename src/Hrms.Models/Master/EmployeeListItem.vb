''' <summary>One row of the employee search grid (mst.usp_Employee_Search).</summary>
Public Class EmployeeListItem
    Public Property EmployeeId As Integer
    Public Property CompanyId As Integer
    Public Property EmpCode As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property DepartmentId As Integer?
    Public Property DesignationId As Integer?
    Public Property PayType As PayType = PayType.MonthlySalary
    Public Property DateOfJoining As Date
    Public Property IsActive As Boolean = True
End Class
