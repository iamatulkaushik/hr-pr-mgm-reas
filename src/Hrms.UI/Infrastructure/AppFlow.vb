Imports System.Windows.Forms
Imports Hrms.Models
Imports Hrms.Models.Company

''' <summary>
''' Start-up and sign-in flow: login -> forced password change -> company choice -> main window.
''' Program.vb calls SignIn() before Application.Run(New frmMain()); frmMain calls ChangeCompany() on F2.
''' </summary>
Public NotInheritable Class AppFlow

    Private Sub New()
    End Sub

    ''' <summary>Returns True when a user and a company are active, False when the user exits.</summary>
    Public Shared Function SignIn() As Boolean
        Do
            Dim session As UserSession = AskLogin()
            If session Is Nothing Then Return False

            If session.MustChangePassword AndAlso Not AskNewPassword(session) Then
                AppServices.Auth.Logout(session.UserId)
                Continue Do
            End If

            Dim company As CompanyInfo = PickCompany(session, Nothing)
            If company Is Nothing Then
                AppServices.Auth.Logout(session.UserId)
                Continue Do
            End If

            AppSession.Begin(session, company)
            Return True
        Loop
    End Function

    ''' <summary>F2: choose another company without signing out. Returns True when it changed.</summary>
    Public Shared Function ChangeCompany(owner As IWin32Window) As Boolean
        Dim current As CompanyInfo = AppSession.Company
        Dim company As CompanyInfo = PickCompany(AppSession.User, current?.CompanyId, owner)
        If company Is Nothing OrElse (current IsNot Nothing AndAlso company.CompanyId = current.CompanyId) Then Return False
        AppSession.Company = company
        Return True
    End Function

    ''' <summary>Signs out (menu, or from the lock screen). frmMain then calls SignIn() again.</summary>
    Public Shared Sub SignOut()
        If AppSession.IsSignedIn Then AppServices.Auth.Logout(AppSession.User.UserId)
        AppSession.End()
    End Sub

    Private Shared Function AskLogin() As UserSession
        Using form As New frmLogin()
            If form.ShowDialog() <> DialogResult.OK Then Return Nothing
            Return form.Session
        End Using
    End Function

    Private Shared Function AskNewPassword(session As UserSession) As Boolean
        Using form As New frmChangePassword(session, forced:=True)
            Return form.ShowDialog() = DialogResult.OK
        End Using
    End Function

    ''' <summary>
    ''' One assigned company is taken without asking (a Company user has exactly one).
    ''' The chosen company is made active in the BLL so the DAL can set SESSION_CONTEXT('CompanyID').
    ''' </summary>
    Private Shared Function PickCompany(session As UserSession, currentCompanyId As Integer?,
                                        Optional owner As IWin32Window = Nothing) As CompanyInfo
        Dim companies As IReadOnlyList(Of CompanyInfo) = AppServices.Auth.GetAssignedCompanies(session.UserId)
        If companies.Count = 0 Then
            Msg.Warn(owner, "No company is assigned to this user. Contact your Associate.")
            Return Nothing
        End If

        Dim chosen As CompanyInfo
        If companies.Count = 1 Then
            chosen = companies(0)
        Else
            Using form As New frmCompanySelect(companies, currentCompanyId)
                If form.ShowDialog(owner) <> DialogResult.OK Then Return Nothing
                chosen = form.SelectedCompany
            End Using
        End If

        Dim result As OperationResult = AppServices.Auth.SelectCompany(session.UserId, chosen.CompanyId)
        If Not result.Succeeded Then
            Msg.Warn(owner, result.Message)
            Return Nothing
        End If
        Return chosen
    End Function

End Class
