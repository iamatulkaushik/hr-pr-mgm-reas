Imports Hrms.Models

''' <summary>The signed-in user and the company they are working in. One per sign-in.</summary>
Friend NotInheritable Class SessionContext

    Private _session As UserSession
    Private _company As CompanyInfo

    Public Event CompanyChanged As EventHandler

    Public ReadOnly Property Session As UserSession
        Get
            Return _session
        End Get
    End Property

    ''' <summary>Nothing when the user has no company yet (an Associate before creating the first one).</summary>
    Public ReadOnly Property CurrentCompany As CompanyInfo
        Get
            Return _company
        End Get
    End Property

    Public Sub SignIn(session As UserSession, company As CompanyInfo)
        _session = session
        _company = company
    End Sub

    Public Sub SelectCompany(company As CompanyInfo)
        _company = company
        RaiseEvent CompanyChanged(Me, EventArgs.Empty)
    End Sub

End Class
