Imports Hrms.UI
Imports Xunit

Public Class FinancialYearTests

    <Theory>
    <InlineData(2026, 10, 3, "2026-27")>
    <InlineData(2026, 4, 1, "2026-27")>
    <InlineData(2027, 3, 31, "2026-27")>
    <InlineData(2027, 4, 1, "2027-28")>
    <InlineData(2026, 1, 15, "2025-26")>
    Public Sub LabelFor_UsesAprilToMarchYear(year As Integer, month As Integer, day As Integer, expected As String)
        Assert.Equal(expected, FinancialYear.LabelFor(New Date(year, month, day)))
    End Sub

End Class
