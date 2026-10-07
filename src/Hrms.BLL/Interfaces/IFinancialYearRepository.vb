Imports Hrms.Models

Public Interface IFinancialYearRepository

    Function List(companyId As Integer) As List(Of FinancialYearDto)

    Function Save(item As FinancialYearDto, userId As Integer) As Integer

    Sub SetCurrent(companyId As Integer, financialYearId As Integer, userId As Integer)

End Interface
