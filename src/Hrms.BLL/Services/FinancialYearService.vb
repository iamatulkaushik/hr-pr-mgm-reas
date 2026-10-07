Imports Hrms.Models

Public NotInheritable Class FinancialYearService

    Private ReadOnly _repository As IFinancialYearRepository
    Private ReadOnly _user As IUserContext

    Public Sub New(repository As IFinancialYearRepository, user As IUserContext)
        _repository = repository
        _user = user
    End Sub

    Public Function List() As List(Of FinancialYearDto)
        Guard.Require(_user, AccessModules.FINANCIAL_YEAR, AccessActions.VIEW)
        Return _repository.List(_user.CompanyId)
    End Function

    ' Creates the financial year starting on 1 April of startYear. The first one becomes the current year.
    Public Function Create(startYear As Integer) As Integer
        Guard.Require(_user, AccessModules.FINANCIAL_YEAR, AccessActions.EDIT)
        If startYear < FinancialYearRules.MIN_START_YEAR OrElse startYear > FinancialYearRules.MAX_START_YEAR Then
            Throw New BusinessRuleException($"Start year must be between {FinancialYearRules.MIN_START_YEAR} and {FinancialYearRules.MAX_START_YEAR}.")
        End If
        Dim existing As List(Of FinancialYearDto) = _repository.List(_user.CompanyId)
        Dim startDate As Date = FinancialYearRules.StartOf(startYear)
        If existing.Any(Function(f) f.StartDate.Date = startDate) Then
            Throw New BusinessRuleException($"Financial year {FinancialYearRules.BuildLabel(startYear)} already exists.")
        End If
        Dim item As New FinancialYearDto() With {
            .CompanyId = _user.CompanyId,
            .Label = FinancialYearRules.BuildLabel(startYear),
            .StartDate = startDate,
            .EndDate = FinancialYearRules.EndOf(startYear),
            .IsCurrent = (existing.Count = 0),
            .IsActive = True
        }
        Return _repository.Save(item, _user.UserId)
    End Function

    Public Sub SetCurrent(financialYearId As Integer)
        Guard.Require(_user, AccessModules.FINANCIAL_YEAR, AccessActions.EDIT)
        _repository.SetCurrent(_user.CompanyId, financialYearId, _user.UserId)
    End Sub

End Class
