Imports System.Windows.Forms
Imports Hrms.BLL
Imports Hrms.Models

''' <summary>
''' The whole sign-in sequence: sign in, forced password change on first login, choose the company.
''' Returns False when the user closes the login window. Cancelling a later step goes back to the login window.
''' </summary>
Friend NotInheritable Class LoginFlow

    Private Const CAPTION As String = "Revolution HRMS"

    Private Sub New()
    End Sub

    Public Shared Function Run(auth As IAuthService, context As SessionContext) As Boolean
        Using login As New frmLogin(auth)
            Do
                login.ResetFields()
                If login.ShowDialog() <> DialogResult.OK Then Return False

                Dim session = login.Session
                If session.MustChangePassword AndAlso Not ChangePasswordRequired(auth, session) Then
                    Continue Do
                End If

                Dim company As CompanyInfo = Nothing
                If Not TryChooseCompany(session, company) Then Continue Do

                context.SignIn(session, company)
                Return True
            Loop
        End Using
        Return False
    End Function

    Private Shared Function ChangePasswordRequired(auth As IAuthService, session As UserSession) As Boolean
        Using dialog As New frmChangePassword(auth, session, True)
            Return dialog.ShowDialog() = DialogResult.OK
        End Using
    End Function

    Private Shared Function TryChooseCompany(session As UserSession, ByRef company As CompanyInfo) As Boolean
        company = Nothing
        Dim companies = session.Companies

        If companies.Count = 0 Then
            ' An Associate can create the first company after signing in. Anyone else cannot work without one.
            If session.Kind = UserType.Associate Then Return True
            MessageBox.Show("No company is assigned to your account. Ask an Associate to assign one.",
                            CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Information)
            Return False
        End If

        If companies.Count = 1 Then
            company = companies(0)
            Return True
        End If

        Using dialog As New frmCompanySelect(companies, Nothing)
            If dialog.ShowDialog() <> DialogResult.OK Then Return False
            company = dialog.SelectedCompany
            Return company IsNot Nothing
        End Using
    End Function

End Class
