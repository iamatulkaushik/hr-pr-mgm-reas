Imports Hrms.BLL
Imports Hrms.Models

Public Class frmDepartment
    Inherits BaseSimpleMasterForm

    Private ReadOnly _service As DepartmentService

    Public Sub New(service As DepartmentService, user As IUserContext)
        MyBase.New(user.CanAccess(AccessModules.DEPARTMENT, AccessActions.EDIT))
        _service = service
    End Sub

    Protected Overrides ReadOnly Property EntityName As String
        Get
            Return "Department"
        End Get
    End Property

    Protected Overrides Function LoadItems(includeInactive As Boolean) As List(Of NamedMasterDto)
        Return _service.List(includeInactive)
    End Function

    Protected Overrides Function SaveItem(item As NamedMasterDto) As Integer
        Return _service.Save(item)
    End Function

End Class
