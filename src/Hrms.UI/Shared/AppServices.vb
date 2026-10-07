Imports Hrms.BLL
Imports Hrms.Common
Imports Hrms.Models

''' <summary>Holds the BLL services for the UI. Program.vb sets them once at start-up (Dev Brave's implementations).</summary>
Public NotInheritable Class AppServices

    Private Shared _auth As IAuthService
    Private Shared _logger As IAppLogger

    Private Sub New()
    End Sub

    Public Shared Sub Initialize(auth As IAuthService, logger As IAppLogger)
        _auth = auth
        _logger = logger
    End Sub

    Public Shared ReadOnly Property Auth As IAuthService
        Get
            If _auth Is Nothing Then Throw New InvalidOperationException("AppServices.Initialize was not called.")
            Return _auth
        End Get
    End Property

    Public Shared ReadOnly Property Logger As IAppLogger
        Get
            If _logger Is Nothing Then Throw New InvalidOperationException("AppServices.Initialize was not called.")
            Return _logger
        End Get
    End Property

End Class
