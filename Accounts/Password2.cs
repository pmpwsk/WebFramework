using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Contains all the needed data of a user's password and data about how the hash should be generated.
/// </summary>
[DataContract]
public class Password2
{
    /// <summary>
    /// The hash algorithm to use for newly set passwords.<br/>
    /// Default: SHA512
    /// </summary>
    public static KeyDerivationPrf DefaultAlgorithm = KeyDerivationPrf.HMACSHA512;

    /// <summary>
    /// The amount of times the password should be hashed for newly set passwords.<br/>
    /// Default: 1048576 = 2^20
    /// </summary>
    public static int DefaultPassCount = 1048576;

    /// <summary>
    /// The length of the salt for newly set passwords.<br/>
    /// Default: 16
    /// </summary>
    public static int DefaultSaltLength = 16;

    /// <summary>
    /// The length (in bytes) of the hash that gets saved.<br/>
    /// This should correspond to the specified hash algorithm!<br/>
    /// Default: 64
    /// </summary>
    public static int DefaultHashLength = 64;

    /// <summary>
    /// Calculates the hash of the given password using the default hashing parameters.<br/>
    /// This is used so a potential attacker can't find out whether an account with a given username exists based on the response time.
    /// </summary>
    public static void WasteTime(string password)
    {
        KeyDerivation.Pbkdf2(password, new byte[] { 1, 2, 3, 4, 5 }, DefaultAlgorithm, DefaultPassCount, DefaultHashLength);
    }

    /// <summary>
    /// The hash algorithm to use.
    /// </summary>
    [DataMember] public readonly KeyDerivationPrf Algorithm;

    /// <summary>
    /// The amount of times the password should be hashed.
    /// </summary>
    [DataMember] public readonly int PassCount;

    /// <summary>
    /// The salt (adds randomness so two hashes of the same password aren't identical).
    /// </summary>
    [DataMember] public readonly byte[] Salt;

    /// <summary>
    /// The hash of the password+salt according to the hashing parameters.
    /// </summary>
    [DataMember] public readonly byte[] Hash;

    /// <summary>
    /// Creates a new password hash object for the given password according to the default hashing parameters.
    /// </summary>
    public Password2(string password)
    {
        Algorithm = DefaultAlgorithm;
        PassCount = DefaultPassCount;
        Salt = RandomNumberGenerator.GetBytes(DefaultSaltLength);
        Hash = KeyDerivation.Pbkdf2(password, Salt, Algorithm, PassCount, DefaultHashLength);
    }

    /// <summary>
    /// Checks whether the given password matches the one that is saved in this object (at least whether the resulting hash matches).
    /// </summary>
    public bool Check(string password)
        => Hash.SequenceEqual(KeyDerivation.Pbkdf2(password, Salt, Algorithm, PassCount, Hash.Length));
}
