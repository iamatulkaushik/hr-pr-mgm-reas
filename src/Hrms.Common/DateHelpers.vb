Imports System.Globalization

''' <summary>Display format dd-MM-yyyy. Financial year runs 1 April to 31 March.</summary>
Public Module DateHelpers

    Public Const DISPLAY_DATE_FORMAT As String = "dd-MM-yyyy"

    Public Function ToDisplay(value As Date) As String
        Return value.ToString(DISPLAY_DATE_FORMAT, CultureInfo.InvariantCulture)
    End Function

    Public Function FinancialYearStart(value As Date) As Date
        Dim startYear = If(value.Month >= 4, value.Year, value.Year - 1)
        Return New Date(startYear, 4, 1)
    End Function

    Public Function FinancialYearEnd(value As Date) As Date
        Return FinancialYearStart(value).AddYears(1).AddDays(-1)
    End Function

    ''' <summary>For example 2026-27.</summary>
    Public Function FinancialYearLabel(value As Date) As String
        Dim startYear = FinancialYearStart(value).Year
        Dim endShort = (startYear + 1) Mod 100
        Return startYear.ToString(CultureInfo.InvariantCulture) & "-" & endShort.ToString("00", CultureInfo.InvariantCulture)
    End Function

End Module
