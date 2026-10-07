''' <summary>Company master record (mst.usp_Company_Get / usp_Company_List).</summary>
Public Class Company
    Public Property CompanyId As Integer
    Public Property Name As String = String.Empty
    Public Property Address As String = String.Empty
    Public Property Pan As String = String.Empty
    Public Property Tan As String = String.Empty
    Public Property EsiCode As String = String.Empty
    Public Property EpfCode As String = String.Empty
    Public Property Lin As String = String.Empty
    Public Property IsActive As Boolean = True
End Class
