Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions

''' <summary>Reads NNNN_name.sql files from a folder, ordered by number.</summary>
Public NotInheritable Class ScriptLoader
    Private Const FILE_PATTERN As String = "^(?<version>\d{4})_[A-Za-z0-9_]+\.sql$"
    Private Shared ReadOnly _fileName As New Regex(FILE_PATTERN, RegexOptions.Compiled)

    Private Sub New()
    End Sub

    Public Shared Function Load(folder As String) As List(Of MigrationScript)
        If Not Directory.Exists(folder) Then Throw New MigrationException($"Scripts folder not found: {folder}")

        Dim scripts As New List(Of MigrationScript)
        For Each path As String In Directory.GetFiles(folder, "*.sql")
            scripts.Add(ReadScript(path))
        Next
        EnsureUniqueVersions(scripts)
        Return scripts.OrderBy(Function(s) s.Version).ToList()
    End Function

    Private Shared Function ReadScript(path As String) As MigrationScript
        Dim fileName As String = System.IO.Path.GetFileName(path)
        Dim match As Match = _fileName.Match(fileName)
        If Not match.Success Then
            Throw New MigrationException($"Bad script name '{fileName}'. Use four digits, underscore, name: 0001_init.sql")
        End If
        Dim sql As String = File.ReadAllText(path, Encoding.UTF8)
        Return New MigrationScript With {
            .Version = Integer.Parse(match.Groups("version").Value),
            .FileName = fileName, .Sql = sql, .Checksum = ScriptChecksum.Compute(sql)}
    End Function

    Private Shared Sub EnsureUniqueVersions(scripts As List(Of MigrationScript))
        Dim duplicate = scripts.GroupBy(Function(s) s.Version).FirstOrDefault(Function(g) g.Count() > 1)
        If duplicate IsNot Nothing Then
            Throw New MigrationException($"Two scripts share number {duplicate.Key:0000}: {String.Join(", ", duplicate.Select(Function(s) s.FileName))}")
        End If
    End Sub
End Class
