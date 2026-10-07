Imports System.Globalization
Imports System.IO
Imports System.Text.Json
Imports System.Threading
Imports System.Windows.Forms
Imports Hrms.Common

Public Module Program

    Private Const APP_CULTURE As String = "en-IN"
    Private Const DATE_FORMAT As String = "dd-MM-yyyy"
    Private Const LOG_ID_LENGTH As Integer = 8
    Private Const APP_TITLE As String = "Revolution HRMS"
    Private _logger As FileLogger

    <STAThread>
    Public Sub Main()
        Dim culture As New CultureInfo(APP_CULTURE)
        culture.DateTimeFormat.ShortDatePattern = DATE_FORMAT
        CultureInfo.DefaultThreadCurrentCulture = culture
        CultureInfo.DefaultThreadCurrentUICulture = culture
        Thread.CurrentThread.CurrentCulture = culture

        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)
        AddHandler Application.ThreadException, Sub(sender, e) ShowFriendlyError()
        AddHandler AppDomain.CurrentDomain.UnhandledException, Sub(sender, e) ShowFriendlyError()

        Application.Run(New frmMain())
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)
        Application.SetHighDpiMode(HighDpiMode.SystemAware)
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException)

        Dim config = LoadConfig()
        If config Is Nothing Then Return

        _logger = New FileLogger(config.LogFolder)
        AddHandler Application.ThreadException, AddressOf OnThreadException
        AddHandler AppDomain.CurrentDomain.UnhandledException, AddressOf OnUnhandledException

        _logger.Info("Application started")
        Application.Run(New frmMain(config))
        _logger.Info("Application closed")

    End Sub

    Private Sub ShowFriendlyError()
        Dim logId As String = Guid.NewGuid().ToString("N").Substring(0, LOG_ID_LENGTH).ToUpperInvariant()
        MessageBox.Show("Something went wrong. Please try again, or contact support." & vbCrLf & "Log ID: " & logId,
                        "Revolution HRMS", MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub
    Private Function LoadConfig() As AppConfig
        Dim path = System.IO.Path.Combine(AppContext.BaseDirectory, AppConfig.FILE_NAME)
        Try
            Return AppConfig.Load(path)
        Catch ex As FileNotFoundException
            MessageBox.Show(ex.Message, APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error)
        Catch ex As JsonException
            MessageBox.Show("appsettings.json is not valid JSON. Check it against appsettings.example.json.",
                            APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
        Return Nothing
    End Function

    Private Sub OnThreadException(sender As Object, e As ThreadExceptionEventArgs)
        Report(e.Exception)
    End Sub

    Private Sub OnUnhandledException(sender As Object, e As UnhandledExceptionEventArgs)
        Dim ex = TryCast(e.ExceptionObject, Exception)
        If ex IsNot Nothing Then Report(ex)
    End Sub

    ' The detail goes to the log. The user sees a short message with a reference, never a stack trace.
    Private Sub Report(ex As Exception)
        Dim reference = _logger.LogError("Unhandled exception", ex)
        MessageBox.Show($"Something went wrong. Please contact support and quote reference {reference}.",
                        APP_TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub
End Module
