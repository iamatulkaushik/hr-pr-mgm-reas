Public Class MigrationPlan
    Public ReadOnly Property Pending As List(Of MigrationScript)
    Public ReadOnly Property AppliedCount As Integer

    Public Sub New(pending As List(Of MigrationScript), appliedCount As Integer)
        Me.Pending = pending
        Me.AppliedCount = appliedCount
    End Sub

    ''' <summary>True when nothing was applied before: a new database needs no backup.</summary>
    Public ReadOnly Property IsFirstRun As Boolean
        Get
            Return AppliedCount = 0
        End Get
    End Property
End Class
