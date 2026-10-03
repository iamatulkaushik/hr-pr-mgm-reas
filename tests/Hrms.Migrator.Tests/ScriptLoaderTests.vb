Imports System.Linq
Imports System.IO
Imports Hrms.Migrator
Imports Xunit

Public Class ScriptLoaderTests
    Implements IDisposable

    Private ReadOnly _folder As String = Path.Combine(Path.GetTempPath(), "hrms-loader-" & Guid.NewGuid().ToString("N"))

    Public Sub New()
        Directory.CreateDirectory(_folder)
    End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        Directory.Delete(_folder, True)
    End Sub

    Private Sub Write(fileName As String, Optional text As String = "SELECT 1;")
        File.WriteAllText(Path.Combine(_folder, fileName), text)
    End Sub

    <Fact>
    Public Sub LoadsInNumericOrder()
        Write("0010_c.sql")
        Write("0002_b.sql")
        Write("0001_a.sql")
        Dim versions As List(Of Integer) = ScriptLoader.Load(_folder).Select(Function(s) s.Version).ToList()
        Assert.Equal(New List(Of Integer) From {1, 2, 10}, versions)
    End Sub

    <Fact>
    Public Sub ComputesAChecksumForEachScript()
        Write("0001_a.sql", "SELECT 42;")
        Assert.Equal(ScriptChecksum.Compute("SELECT 42;"), ScriptLoader.Load(_folder)(0).Checksum)
    End Sub

    <Theory>
    <InlineData("1_init.sql")>
    <InlineData("0001-init.sql")>
    <InlineData("0001_init script.sql")>
    <InlineData("init.sql")>
    Public Sub RejectsBadFileNames(fileName As String)
        Write(fileName)
        Assert.Throws(Of MigrationException)(Function() ScriptLoader.Load(_folder))
    End Sub

    <Fact>
    Public Sub RejectsTwoScriptsWithTheSameNumber()
        Write("0001_a.sql")
        Write("0001_b.sql")
        Assert.Throws(Of MigrationException)(Function() ScriptLoader.Load(_folder))
    End Sub

    <Fact>
    Public Sub RejectsMissingFolder()
        Assert.Throws(Of MigrationException)(Function() ScriptLoader.Load(Path.Combine(_folder, "nope")))
    End Sub
End Class
