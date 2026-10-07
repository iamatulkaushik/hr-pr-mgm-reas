Option Strict On

Public NotInheritable Class PasswordPolicy
    Public Const MinLength As Integer = 8

    ''' <summary>Returns Nothing when the password is acceptable, else the reason.</summary>
    Public Shared Function Validate(password As String, username As String) As String
        If String.IsNullOrEmpty(password) OrElse password.Length < MinLength Then Return $"Password must be at least {MinLength} characters."
        If Not password.Any(AddressOf Char.IsUpper) Then Return "Password needs an uppercase letter."
        If Not password.Any(AddressOf Char.IsLower) Then Return "Password needs a lowercase letter."
        If Not password.Any(AddressOf Char.IsDigit) Then Return "Password needs a digit."
        If password.All(AddressOf Char.IsLetterOrDigit) Then Return "Password needs a special character."
        If Not String.IsNullOrEmpty(username) AndAlso password.IndexOf(username, StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return "Password must not contain the username."
        End If
        Return Nothing
    End Function
End Class
