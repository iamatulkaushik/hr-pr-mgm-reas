Imports System.Globalization

''' <summary>Indian number formatting: ₹ 1,23,456.00</summary>
Public Module IndianFormat

    Public Function ToRupees(amount As Decimal) As String
        Dim rounded = Math.Round(amount, 2, MidpointRounding.AwayFromZero)
        Dim text = Math.Abs(rounded).ToString("F2", CultureInfo.InvariantCulture)
        Dim dot = text.IndexOf("."c)
        Dim whole = text.Substring(0, dot)
        Dim fraction = text.Substring(dot)
        Dim sign = If(rounded < 0D, "-", String.Empty)
        Return sign & "₹ " & GroupDigits(whole) & fraction
    End Function

    ' Last three digits together, then pairs from the right: 12345678 -> 1,23,45,678
    Private Function GroupDigits(digits As String) As String
        If digits.Length <= 3 Then Return digits

        Dim lastThree = digits.Substring(digits.Length - 3)
        Dim rest = digits.Substring(0, digits.Length - 3)
        Dim groups As New List(Of String)()
        Dim position = rest.Length
        While position > 0
            Dim start = Math.Max(0, position - 2)
            groups.Insert(0, rest.Substring(start, position - start))
            position = start
        End While
        Return String.Join(",", groups) & "," & lastThree
    End Function

End Module
