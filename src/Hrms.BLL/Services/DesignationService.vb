Imports Hrms.Models

Public NotInheritable Class DesignationService

    Private Const LABEL As String = "Designation"

    Private ReadOnly _repository As INamedMasterRepository
    Private ReadOnly _user As IUserContext

    Public Sub New(repository As INamedMasterRepository, user As IUserContext)
        _repository = repository
        _user = user
    End Sub

    Public Function List(includeInactive As Boolean) As List(Of NamedMasterDto)
        Guard.Require(_user, AccessModules.DESIGNATION, AccessActions.VIEW)
        Return _repository.List(_user.CompanyId, includeInactive)
    End Function

    Public Function Save(item As NamedMasterDto) As Integer
        Guard.Require(_user, AccessModules.DESIGNATION, AccessActions.EDIT)
        item.Name = NamedMasterRules.CleanName(item.Name)
        NamedMasterRules.EnsureValid(item, _repository.List(_user.CompanyId, True), LABEL)
        item.CompanyId = _user.CompanyId
        Return _repository.Save(item, _user.UserId)
    End Function

End Class
