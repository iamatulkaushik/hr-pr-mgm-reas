Imports System.Globalization

Public NotInheritable Class FinancialYear

    Private Const FY_START_MONTH As Integer = 4

    Private Sub New()
    End Sub

    Public Shared Function LabelFor(onDate As Date) As String
        Dim startYear As Integer = If(onDate.Month >= FY_START_MONTH, onDate.Year, onDate.Year - 1)
        Dim endYY As Integer = (startYear + 1) Mod 100
        Return startYear.ToString(CultureInfo.InvariantCulture) & "-" & endYY.ToString("00", CultureInfo.InvariantCulture)
    End Function

End Class
