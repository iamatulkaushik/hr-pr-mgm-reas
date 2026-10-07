''' <summary>
''' One place where modules register their forms with the shell.
''' Example when the employee screen ships:
'''   registry.Register("masters.employee", Function() New frmEmployeeList())
''' </summary>
Friend Module ModuleRegistrations

    Public Sub RegisterAll(registry As FormRegistry)
        ' No module screens exist yet in Phase 1. The menu shows the items the user may open, disabled.
    End Sub

End Module
