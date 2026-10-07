
''' <summary>Agreed contract. Implementation: Dev Brave (rolling file log). Never log passwords or IDs.</summary>
Public Interface IAppLogger
    ''' <summary>Logs the exception and returns a short log ID to show the user.</summary>
    Function LogError(ex As Exception) As String
End Interface

