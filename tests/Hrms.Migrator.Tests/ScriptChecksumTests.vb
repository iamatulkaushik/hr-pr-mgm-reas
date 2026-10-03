Imports Hrms.Migrator
Imports Xunit

Public Class ScriptChecksumTests
    <Fact>
    Public Sub LineEndingStyleDoesNotChangeTheChecksum()
        Assert.Equal(ScriptChecksum.Compute("A" & vbLf & "B"), ScriptChecksum.Compute("A" & vbCrLf & "B"))
    End Sub

    <Fact>
    Public Sub ByteOrderMarkIsIgnored()
        Assert.Equal(ScriptChecksum.Compute("SELECT 1;"), ScriptChecksum.Compute(ChrW(&HFEFF) & "SELECT 1;"))
    End Sub

    <Fact>
    Public Sub DifferentTextGivesDifferentChecksum()
        Assert.NotEqual(ScriptChecksum.Compute("SELECT 1;"), ScriptChecksum.Compute("SELECT 2;"))
    End Sub

    <Fact>
    Public Sub ChecksumIs64LowercaseHexCharacters()
        Dim value As String = ScriptChecksum.Compute("x")
        Assert.Equal(64, value.Length)
        Assert.Equal(value.ToLowerInvariant(), value)
    End Sub
End Class
