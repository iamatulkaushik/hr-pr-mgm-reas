''' <summary>
''' Full employee record (mst.usp_Employee_Get). The encrypted ID fields (Aadhaar, PAN, bank, UAN, ESI IP)
''' are not part of this release. Leave EmpCode empty on a new employee to get the next code.
''' </summary>
Public Class Employee
    Public Property EmployeeId As Integer
    Public Property CompanyId As Integer
    Public Property EmpCode As String = String.Empty
    Public Property Name As String = String.Empty
    Public Property FatherName As String = String.Empty
    Public Property DateOfBirth As Date?
    Public Property Gender As String = String.Empty
    Public Property DateOfJoining As Date
    Public Property DateOfLeaving As Date?
    Public Property FactoryId As Integer?
    Public Property DepartmentId As Integer?
    Public Property DesignationId As Integer?
    Public Property SkillCategory As String = String.Empty
    Public Property PayType As PayType = PayType.MonthlySalary
    Public Property Ifsc As String = String.Empty
    Public Property Mobile As String = String.Empty
    Public Property Address As String = String.Empty
    Public Property IsActive As Boolean = True
End Class
