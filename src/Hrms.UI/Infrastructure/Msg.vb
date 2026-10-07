Imports System.Windows.Forms

''' <summary>
''' Message styles from DESIGN.md: info = blue icon; warning = amber icon and confirms the action;
''' error = red icon, short text and a log ID, never a stack trace; success = status bar text for 3 seconds.
''' </summary>
Public NotInheritable Class Msg

    Public Const SUCCESS_STATUS_SECONDS As Integer = 3
    Private Const TITLE As String = "Revolution HRMS"

    ''' <summary>frmMain subscribes and shows the text in the status bar for the given seconds.</summary>
    Public Shared Event StatusTextRequested(text As String, seconds As Integer)

    Private Sub New()
    End Sub

    Public Shared Sub Info(owner As IWin32Window, text As String)
        MessageBox.Show(owner, text, TITLE, MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub

    Public Shared Sub Warn(owner As IWin32Window, text As String)
        MessageBox.Show(owner, text, TITLE, MessageBoxButtons.OK, MessageBoxIcon.Warning)
    End Sub

    Public Shared Function Confirm(owner As IWin32Window, text As String) As Boolean
        Return MessageBox.Show(owner, text, TITLE, MessageBoxButtons.YesNo,
                               MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) = DialogResult.Yes
    End Function

    Public Shared Sub ShowError(owner As IWin32Window, text As String, logId As String)
        MessageBox.Show(owner, text & Environment.NewLine & Environment.NewLine & "Log ID: " & logId,
                        TITLE, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub

    Public Shared Sub Success(text As String)
        RaiseEvent StatusTextRequested(text, SUCCESS_STATUS_SECONDS)
    End Sub

End Class
