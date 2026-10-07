Public Class FinancialYearDto

    Public Property Id As Integer
    Public Property CompanyId As Integer
    Public Property Label As String = String.Empty
    Public Property StartDate As Date
    Public Property EndDate As Date
    Public Property IsCurrent As Boolean
    Public Property IsActive As Boolean = True

End Class
