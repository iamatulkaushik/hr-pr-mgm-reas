Imports Hrms.Models

Public Interface IHolidayRepository

    Function List(companyId As Integer, fromDate As Date, toDate As Date) As List(Of HolidayDto)

    Function Save(item As HolidayDto, userId As Integer) As Integer

    Sub Deactivate(companyId As Integer, holidayId As Integer, userId As Integer)

End Interface
