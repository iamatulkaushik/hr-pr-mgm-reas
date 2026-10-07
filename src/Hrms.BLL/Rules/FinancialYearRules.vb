Public NotInheritable Class FinancialYearRules

    Public Const MIN_START_YEAR As Integer = 2000
    Public Const MAX_START_YEAR As Integer = 2100
    Private Const START_MONTH As Integer = 4
    Private Const END_MONTH As Integer = 3
    Private Const END_DAY As Integer = 31
    Private Const CENTURY As Integer = 100

    Private Sub New()
    End Sub

    Public Shared Function StartOf(startYear As Integer) As Date
        Return New Date(startYear, START_MONTH, 1)
    End Function

    Public Shared Function EndOf(startYear As Integer) As Date
        Return New Date(startYear + 1, END_MONTH, END_DAY)
    End Function

    Public Shared Function BuildLabel(startYear As Integer) As String
        Dim endShort As Integer = (startYear + 1) Mod CENTURY
        Return $"{startYear}-{endShort:00}"
    End Function

End Class
