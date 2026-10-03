Imports Hrms.Migrator
Imports Xunit

Public Class SqlBatchSplitterTests
    <Fact>
    Public Sub SplitsOnGoLinesInAnyCase()
        Dim batches As List(Of String) = SqlBatchSplitter.Split("SELECT 1;" & vbLf & "GO" & vbLf & "SELECT 2;" & vbLf & "go" & vbLf & "SELECT 3;")
        Assert.Equal(3, batches.Count)
        Assert.Equal("SELECT 2;", batches(1))
    End Sub

    <Fact>
    Public Sub DropsEmptyBatches()
        Dim batches As List(Of String) = SqlBatchSplitter.Split("GO" & vbLf & vbLf & "GO" & vbLf & "SELECT 1;" & vbLf & "GO" & vbLf)
        Assert.Single(batches)
    End Sub

    <Fact>
    Public Sub OnlyAWholeLineOfGoSplits()
        Dim batches As List(Of String) = SqlBatchSplitter.Split("SELECT 'GOOD';" & vbLf & "GOTO done;" & vbLf & "-- GO")
        Assert.Single(batches)
    End Sub

    <Fact>
    Public Sub HandlesWindowsLineEndingsAndSpaces()
        Dim batches As List(Of String) = SqlBatchSplitter.Split("SELECT 1;" & vbCrLf & "  GO  " & vbCrLf & "SELECT 2;")
        Assert.Equal(2, batches.Count)
    End Sub
End Class
