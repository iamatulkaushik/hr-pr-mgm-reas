Imports System.Drawing
Imports System.Windows.Forms

Public Class frmMain
    Inherits Form

    Private Const MESSAGE_SECONDS As Integer = 3
    Private Const NOT_SET As String = "-"

    Private Shared ReadOnly MenuNames As String() = {"Masters", "Attendance", "Payroll", "Statutory", "Career", "Reports", "Admin"}

    Private ReadOnly _menu As New MenuStrip()
    Private ReadOnly _toolbar As New ToolStrip()
    Private ReadOnly _status As New StatusStrip()
    Private ReadOnly _lblMessage As New ToolStripStatusLabel()
    Private ReadOnly _lblUser As New ToolStripStatusLabel()
    Private ReadOnly _lblCompany As New ToolStripStatusLabel()
    Private ReadOnly _lblFy As New ToolStripStatusLabel()
    Private ReadOnly _lblServer As New ToolStripStatusLabel()
    Private ReadOnly _lblRole As New ToolStripStatusLabel()
    Private ReadOnly _messageTimer As New Timer()

    Public Event CompanySelectRequested As EventHandler

    Public Sub New()
        Text = "Revolution HRMS"
        IsMdiContainer = True
        WindowState = FormWindowState.Maximized
        Font = AppTheme.BaseFont
        BackColor = AppTheme.Background

        BuildMenu()
        BuildToolbar()
        BuildStatusBar()
        MainMenuStrip = _menu
        Controls.Add(_status)
        Controls.Add(_toolbar)
        Controls.Add(_menu)

        ShowSession(NOT_SET, NOT_SET, NOT_SET, NOT_SET)
        _messageTimer.Interval = MESSAGE_SECONDS * 1000
        AddHandler _messageTimer.Tick, AddressOf OnMessageTimerTick
    End Sub

    Public Sub ShowSession(userName As String, companyName As String, serverName As String, roleName As String)
        _lblUser.Text = "User: " & userName
        _lblCompany.Text = "Company: " & companyName
        _lblFy.Text = "FY " & FinancialYear.LabelFor(Date.Today)
        _lblServer.Text = "Server: " & serverName
        _lblRole.Text = "Role: " & roleName
    End Sub

    Public Sub ShowSuccess(message As String)
        _lblMessage.Text = message
        _messageTimer.Stop()
        _messageTimer.Start()
    End Sub

    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        For Each child As Control In Controls
            If TypeOf child Is MdiClient Then child.BackColor = AppTheme.Background
        Next
    End Sub

    Protected Overrides Function ProcessCmdKey(ByRef msg As Message, keyData As Keys) As Boolean
        If Hotkeys.Resolve(keyData) = HotkeyAction.SelectCompany Then
            RaiseEvent CompanySelectRequested(Me, EventArgs.Empty)
            Return True
        End If
        Return MyBase.ProcessCmdKey(msg, keyData)
    End Function

    Protected Overrides Sub Dispose(disposing As Boolean)
        If disposing Then _messageTimer.Dispose()
        MyBase.Dispose(disposing)
    End Sub

    Private Sub BuildMenu()
        _menu.BackColor = AppTheme.Primary
        _menu.ForeColor = Color.White
        For Each caption As String In MenuNames
            _menu.Items.Add(New ToolStripMenuItem(caption))
        Next
        Dim adminMenu = DirectCast(_menu.Items(MenuNames.Length - 1), ToolStripMenuItem)
        Dim exitItem As New ToolStripMenuItem("Exit") With {.ForeColor = SystemColors.ControlText}
        AddHandler exitItem.Click, Sub(sender, e) Close()
        adminMenu.DropDownItems.Add(exitItem)
    End Sub

    Private Sub BuildToolbar()
        _toolbar.GripStyle = ToolStripGripStyle.Hidden
        _toolbar.BackColor = Color.White
        AddTool("New", HotkeyAction.NewRecord)
        AddTool("Save", HotkeyAction.Save)
        AddTool("Delete", HotkeyAction.DeleteRecord)
        AddTool("Find", HotkeyAction.Find)
        AddTool("Print", HotkeyAction.Print)
        AddTool("Export", HotkeyAction.Export)
    End Sub

    Private Sub AddTool(caption As String, action As HotkeyAction)
        Dim button As New ToolStripButton(caption)
        AddHandler button.Click, Sub(sender, e) RunOnActiveChild(action)
        _toolbar.Items.Add(button)
    End Sub

    Private Sub BuildStatusBar()
        _status.BackColor = AppTheme.Primary
        _lblMessage.Spring = True
        _lblMessage.TextAlign = ContentAlignment.MiddleLeft
        Dim labels As ToolStripStatusLabel() = {_lblMessage, _lblUser, _lblCompany, _lblFy, _lblServer, _lblRole}
        For Each label As ToolStripStatusLabel In labels
            label.ForeColor = Color.White
            _status.Items.Add(label)
        Next
    End Sub

    Private Sub RunOnActiveChild(action As HotkeyAction)
        Dim child = TryCast(ActiveMdiChild, BaseForm)
        If child IsNot Nothing Then child.Execute(action)
    End Sub

    Private Sub OnMessageTimerTick(sender As Object, e As EventArgs)
        _messageTimer.Stop()
        _lblMessage.Text = String.Empty
    End Sub

End Class
