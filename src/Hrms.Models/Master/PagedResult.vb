''' <summary>One page of a list plus the total number of rows before paging.</summary>
Public Class PagedResult(Of T)

    Public Sub New(items As IReadOnlyList(Of T), totalRows As Integer, pageNumber As Integer, pageSize As Integer)
        Me.Items = items
        Me.TotalRows = totalRows
        Me.PageNumber = pageNumber
        Me.PageSize = pageSize
    End Sub

    Public ReadOnly Property Items As IReadOnlyList(Of T)
    Public ReadOnly Property TotalRows As Integer
    Public ReadOnly Property PageNumber As Integer
    Public ReadOnly Property PageSize As Integer

End Class
