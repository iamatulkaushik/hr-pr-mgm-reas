Imports Hrms.Models

Public NotInheritable Class HolidayService

    Private Const MAX_NAME_LENGTH As Integer = 100

    Private ReadOnly _repository As IHolidayRepository
    Private ReadOnly _user As IUserContext

    Public Sub New(repository As IHolidayRepository, user As IUserContext)
        _repository = repository
        _user = user
    End Sub

    Public Function List(fromDate As Date, toDate As Date) As List(Of HolidayDto)
        Guard.Require(_user, AccessModules.HOLIDAY, AccessActions.VIEW)
        Return _repository.List(_user.CompanyId, fromDate.Date, toDate.Date)
    End Function

    Public Function Save(item As HolidayDto, fyStart As Date, fyEnd As Date) As Integer
        Guard.Require(_user, AccessModules.HOLIDAY, AccessActions.EDIT)
        item.Name = NamedMasterRules.CleanName(item.Name)
        If item.Name.Length = 0 Then
            Throw New BusinessRuleException("Holiday name is required.")
        End If
        If item.Name.Length > MAX_NAME_LENGTH Then
            Throw New BusinessRuleException($"Holiday name cannot be longer than {MAX_NAME_LENGTH} characters.")
        End If
        If Array.IndexOf(HolidayTypes.All, item.HolidayType) < 0 Then
            Throw New BusinessRuleException("Choose a valid holiday type.")
        End If
        If item.HolidayDate.Date < fyStart.Date OrElse item.HolidayDate.Date > fyEnd.Date Then
            Throw New BusinessRuleException($"The date must fall within the financial year ({fyStart:dd-MM-yyyy} to {fyEnd:dd-MM-yyyy}).")
        End If
        Dim sameDay As HolidayDto = _repository.List(_user.CompanyId, item.HolidayDate.Date, item.HolidayDate.Date) _
            .FirstOrDefault(Function(h) h.Id <> item.Id)
        If sameDay IsNot Nothing Then
            Throw New BusinessRuleException($"{item.HolidayDate:dd-MM-yyyy} is already marked as '{sameDay.Name}'.")
        End If
        item.CompanyId = _user.CompanyId
        Return _repository.Save(item, _user.UserId)
    End Function

    Public Sub Deactivate(holidayId As Integer)
        Guard.Require(_user, AccessModules.HOLIDAY, AccessActions.EDIT)
        _repository.Deactivate(_user.CompanyId, holidayId, _user.UserId)
    End Sub

End Class
