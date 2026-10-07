Option Strict On
Imports System.Security.Cryptography
Imports System.Text

''' <summary>PBKDF2 password hashing and AES-256-GCM field encryption.</summary>
Public Module CryptoHelper
    Public Const DefaultIterations As Integer = 150000   ' spec minimum is 100,000
    Private Const SaltSize As Integer = 16
    Private Const HashSize As Integer = 32
    Private Const NonceSize As Integer = 12
    Private Const TagSize As Integer = 16

    Public Function NewSalt() As Byte()
        Return RandomNumberGenerator.GetBytes(SaltSize)
    End Function

    Public Function HashPassword(password As String, salt As Byte(), iterations As Integer) As Byte()
        Return Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, HashAlgorithmName.SHA256, HashSize)
    End Function

    Public Function VerifyPassword(password As String, salt As Byte(), iterations As Integer, expected As Byte()) As Boolean
        Dim actual = HashPassword(password, salt, iterations)
        Return CryptographicOperations.FixedTimeEquals(actual, expected)
    End Function

    ''' <summary>Output layout: nonce(12) | tag(16) | cipher.</summary>
    Public Function Encrypt(plainText As String, key As Byte()) As Byte()
        RequireKey(key)
        Dim plain = Encoding.UTF8.GetBytes(plainText)
        Dim nonce = RandomNumberGenerator.GetBytes(NonceSize)
        Dim cipher(plain.Length - 1) As Byte
        Dim tag(TagSize - 1) As Byte
        Using aes As New AesGcm(key, TagSize)
            aes.Encrypt(nonce, plain, cipher, tag)
        End Using
        Dim result(NonceSize + TagSize + cipher.Length - 1) As Byte
        Buffer.BlockCopy(nonce, 0, result, 0, NonceSize)
        Buffer.BlockCopy(tag, 0, result, NonceSize, TagSize)
        Buffer.BlockCopy(cipher, 0, result, NonceSize + TagSize, cipher.Length)
        Return result
    End Function

    Public Function Decrypt(blob As Byte(), key As Byte()) As String
        RequireKey(key)
        If blob Is Nothing OrElse blob.Length < NonceSize + TagSize Then Throw New CryptographicException("Invalid encrypted data.")
        Dim nonce(NonceSize - 1) As Byte
        Dim tag(TagSize - 1) As Byte
        Dim cipher(blob.Length - NonceSize - TagSize - 1) As Byte
        Buffer.BlockCopy(blob, 0, nonce, 0, NonceSize)
        Buffer.BlockCopy(blob, NonceSize, tag, 0, TagSize)
        Buffer.BlockCopy(blob, NonceSize + TagSize, cipher, 0, cipher.Length)
        Dim plain(cipher.Length - 1) As Byte
        Using aes As New AesGcm(key, TagSize)
            aes.Decrypt(nonce, cipher, tag, plain)
        End Using
        Return Encoding.UTF8.GetString(plain)
    End Function

    Private Sub RequireKey(key As Byte())
        If key Is Nothing OrElse key.Length <> 32 Then Throw New ArgumentException("Key must be 32 bytes (AES-256).", NameOf(key))
    End Sub
End Module
