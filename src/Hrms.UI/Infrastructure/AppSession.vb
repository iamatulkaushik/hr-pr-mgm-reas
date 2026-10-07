Imports System.Globalization
Imports Hrms.Models
Imports Hrms.Models.Auth
Imports Hrms.Models.Company

''' <summary>Who is signed in and which company is active. Drives the status bar and the menu.</summary>
Public NotInheritable Class AppSession

    Private Const FY_START_MONTH As Integer = 4   ' financial year runs 1 April to 31 March

    Private Sub New()
    End Sub

    Public Shared Property User As UserSession
    Public Shared Property Company As CompanyInfo
    Public Shared Property ServerName As String = String.Empty

    Public Shared ReadOnly Property IsSignedIn As Boolean
        Get
            Return User IsNot Nothing
        End Get
    End Property

    Public Shared Sub Begin(user As UserSession, company As CompanyInfo)
        AppSession.User = user
        AppSession.Company = company
    End Sub

    Public Shared Sub [End]()
        User = Nothing
        Company = Nothing
    End Sub

    Public Shared Function CanAccess(moduleName As String, action As String) As Boolean
        Return IsSignedIn AndAlso AppServices.Auth.CanAccess(User, moduleName, action)
    End Function

    ''' <summary>Financial year label such as "2026-27".</summary>
    Public Shared Function FinancialYearLabel(onDate As Date) As String
        Dim startYear As Integer = If(onDate.Month >= FY_START_MONTH, onDate.Year, onDate.Year - 1)
        Dim endShort As Integer = (startYear + 1) Mod 100
        Return startYear.ToString(CultureInfo.InvariantCulture) & "-" & endShort.ToString("00", CultureInfo.InvariantCulture)
    End Function

End Class
