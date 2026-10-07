' PENDING: needs Dev Brave's PasswordHasher in Revolution.Hrms.Common (not merged yet).
' Not compiled until you build with -p:BraveMerged=true. Contract these tests assume:
'   PasswordHasher.CreateHash(password As String) As PasswordHashResult   (Hash, Salt As Byte(), Iterations As Integer)
'   PasswordHasher.Verify(password, hash, salt, iterations) As Boolean
'   PasswordHasher.DEFAULT_ITERATIONS >= 100000 (PBKDF2, spec FR-1)
' If Brave's names differ, change this file. The rules being tested must not change.
Imports Revolution.Hrms.Common
Imports Xunit

Public Class PasswordHasherPendingTests

    <Fact>
    Public Sub Verify_CorrectPassword_IsTrue()
        Dim result = PasswordHasher.CreateHash("Correct#Horse1")
        Assert.True(PasswordHasher.Verify("Correct#Horse1", result.Hash, result.Salt, result.Iterations))
    End Sub

    <Fact>
    Public Sub Verify_WrongPassword_IsFalse()
        Dim result = PasswordHasher.CreateHash("Correct#Horse1")
        Assert.False(PasswordHasher.Verify("correct#horse1", result.Hash, result.Salt, result.Iterations))
    End Sub

    <Fact>
    Public Sub CreateHash_SamePasswordTwice_GivesDifferentSaltsAndHashes()
        Dim first = PasswordHasher.CreateHash("Same#Password1")
        Dim second = PasswordHasher.CreateHash("Same#Password1")
        Assert.NotEqual(first.Salt, second.Salt)
        Assert.NotEqual(first.Hash, second.Hash)
    End Sub

    <Fact>
    Public Sub CreateHash_UsesAtLeast100000Iterations()
        Assert.True(PasswordHasher.CreateHash("x").Iterations >= 100000)
    End Sub

    <Fact>
    Public Sub Verify_WrongSalt_IsFalse()
        Dim result = PasswordHasher.CreateHash("Correct#Horse1")
        Dim otherSalt = PasswordHasher.CreateHash("anything").Salt
        Assert.False(PasswordHasher.Verify("Correct#Horse1", result.Hash, otherSalt, result.Iterations))
    End Sub

End Class
