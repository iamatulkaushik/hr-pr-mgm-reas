Public Class HolidayDto

    Public Property Id As Integer
    Public Property CompanyId As Integer
    Public Property HolidayDate As Date
    Public Property Name As String = String.Empty
    Public Property HolidayType As String = HolidayTypes.NATIONAL
    Public Property IsActive As Boolean = True

End Class
