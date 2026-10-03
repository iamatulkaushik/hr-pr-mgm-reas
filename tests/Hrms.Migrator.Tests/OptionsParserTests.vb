Imports Hrms.Migrator
Imports Xunit

Public Class OptionsParserTests
    Private Shared Function Env(Optional server As String = Nothing, Optional password As String = Nothing) As Func(Of String, String)
        Return Function(name As String)
                   If name = OptionsParser.ENV_SERVER Then Return server
                   If name = OptionsParser.ENV_PASSWORD Then Return password
                   Return Nothing
               End Function
    End Function

    <Fact>
    Public Sub DefaultsAreApplied()
        Dim o As MigratorOptions = OptionsParser.Parse({"--server", "(localdb)\HRMSDev"}, Env())
        Assert.Equal(MigratorOptions.DEFAULT_MASTER_DATABASE, o.MasterDatabase)
        Assert.Equal(MigratorOptions.DEFAULT_DATA_DATABASE, o.DataDatabase)
        Assert.Equal(MigrationTarget.Both, o.Target)
        Assert.False(o.CreateDatabases)
        Assert.False(o.DryRun)
    End Sub

    <Fact>
    Public Sub ServerCanComeFromTheEnvironment()
        Assert.Equal("SRV\HRMS,14330", OptionsParser.Parse(New String() {}, Env("SRV\HRMS,14330")).Server)
    End Sub

    <Fact>
    Public Sub FlagsAndValuesAreRead()
        Dim o As MigratorOptions = OptionsParser.Parse(
            {"--server", "s", "--create-db", "--dry-run", "--trust-cert", "--target", "data", "--backup-dir", "D:\bk", "--scripts", "db"}, Env())
        Assert.True(o.CreateDatabases)
        Assert.True(o.DryRun)
        Assert.True(o.TrustServerCertificate)
        Assert.Equal(MigrationTarget.Data, o.Target)
        Assert.Equal("D:\bk", o.BackupDirectory)
        Assert.Equal("db", o.ScriptsRoot)
    End Sub

    <Fact>
    Public Sub PasswordIsReadFromTheEnvironmentOnly()
        Dim o As MigratorOptions = OptionsParser.Parse({"--server", "s", "--user", "hrms_admin"}, Env(Nothing, "secret"))
        Assert.Equal("secret", o.Password)
        Assert.Throws(Of UsageException)(Function() OptionsParser.Parse({"--server", "s", "--password", "x"}, Env()))
    End Sub

    <Fact>
    Public Sub SqlLoginWithoutPasswordIsRefused()
        Assert.Throws(Of UsageException)(Function() OptionsParser.Parse({"--server", "s", "--user", "u"}, Env()))
    End Sub

    <Fact>
    Public Sub MissingServerUnknownOptionAndBadValuesAreRefused()
        Assert.Throws(Of UsageException)(Function() OptionsParser.Parse(New String() {}, Env()))
        Assert.Throws(Of UsageException)(Function() OptionsParser.Parse({"--server", "s", "--bogus"}, Env()))
        Assert.Throws(Of UsageException)(Function() OptionsParser.Parse({"--server", "s", "--target", "everything"}, Env()))
        Assert.Throws(Of UsageException)(Function() OptionsParser.Parse({"--server"}, Env()))
    End Sub

    <Theory>
    <InlineData("HRMS_Data; DROP DATABASE x")>
    <InlineData("bad name")>
    <InlineData("")>
    Public Sub UnsafeDatabaseNamesAreRefused(name As String)
        Assert.Throws(Of UsageException)(Function() OptionsParser.Parse({"--server", "s", "--data-db", name}, Env()))
    End Sub
End Class
