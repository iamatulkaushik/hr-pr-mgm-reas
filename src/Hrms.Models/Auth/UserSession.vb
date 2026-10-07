Imports System.Linq

''' <summary>
''' Who is signed in: id, name, user type, permission codes and the companies they may work in.
''' Forms ask CanAccess to hide or disable UI. Services call Require and RequireCompany to enforce.
''' </summary>
Public NotInheritable Class UserSession

    Private ReadOnly _permissions As HashSet(Of String)
    Private _companies As List(Of CompanyInfo)

    Public Sub New(userId As Integer,
                   username As String,
                   kind As UserType,
                   permissionCodes As IEnumerable(Of String),
                   companies As IEnumerable(Of CompanyInfo),
                   mustChangePassword As Boolean)
        Me.UserId = userId
        Me.Username = username
        Me.Kind = kind
        Me.MustChangePassword = mustChangePassword
        _permissions = New HashSet(Of String)(permissionCodes, StringComparer.OrdinalIgnoreCase)
        _companies = companies.ToList()
    End Sub

    Public ReadOnly Property UserId As Integer
    Public ReadOnly Property Username As String
    Public ReadOnly Property Kind As UserType

    ''' <summary>True until the user has changed the temporary password.</summary>
    Public Property MustChangePassword As Boolean

    Public ReadOnly Property Companies As IReadOnlyList(Of CompanyInfo)
        Get
            Return _companies
        End Get
    End Property

    ''' <summary>Used after an Associate creates a company, so the new one shows up without a new login.</summary>
    Public Sub ReplaceCompanies(companies As IEnumerable(Of CompanyInfo))
        _companies = companies.ToList()
    End Sub

    Public Function CanAccess(moduleName As String, action As String) As Boolean
        Return _permissions.Contains(moduleName & "." & action)
    End Function

    Public Sub Require(moduleName As String, action As String)
        If Not CanAccess(moduleName, action) Then
            Throw New AccessDeniedException($"You do not have permission for {moduleName} {action}.")
        End If
    End Sub

    Public Function CanUseCompany(companyId As Integer) As Boolean
        Return _companies.Any(Function(c) c.CompanyId = companyId)
    End Function

    Public Sub RequireCompany(companyId As Integer)
        If Not CanUseCompany(companyId) Then
            Throw New AccessDeniedException("You do not have access to this company.")
        End If
    End Sub

End Class
