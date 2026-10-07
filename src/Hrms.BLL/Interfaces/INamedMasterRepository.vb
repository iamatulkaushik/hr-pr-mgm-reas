Imports Hrms.Models

Public Interface INamedMasterRepository

    Function List(companyId As Integer, includeInactive As Boolean) As List(Of NamedMasterDto)

    Function Save(item As NamedMasterDto, userId As Integer) As Integer

End Interface
