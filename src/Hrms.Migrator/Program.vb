Imports System.Linq
Imports Microsoft.Data.SqlClient

Module Program
    Function Main(args As String()) As Integer
        If args.Contains("--help") OrElse args.Contains("-h") Then
            Console.WriteLine(OptionsParser.UsageText)
            Return ExitCodes.OK
        End If

        Try
            Dim options As MigratorOptions = OptionsParser.Parse(args, AddressOf Environment.GetEnvironmentVariable)
            Dim runner As New MigrationRunner(options, AddressOf Console.WriteLine)
            runner.Run()
            Return ExitCodes.OK
        Catch ex As UsageException
            Console.Error.WriteLine(ex.Message)
            Console.Error.WriteLine(OptionsParser.UsageText)
            Return ExitCodes.USAGE
        Catch ex As MigrationException
            Console.Error.WriteLine("Migration stopped: " & ex.Message)
            Return ExitCodes.FAILED
        Catch ex As SqlException
            Console.Error.WriteLine("Database error: " & ex.Message)
            Return ExitCodes.FAILED
        End Try
    End Function
End Module
