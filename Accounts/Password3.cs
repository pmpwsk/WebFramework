using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Runtime.Serialization;
using System.Security.Cryptography;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Contains all the needed data of a user's password and data about how the hash should be generated.
/// </summary>
[DataContract]
public class Password3
{
    /// <summary>
    /// The possible derivation algorithms.
    /// </summary>
    public enum DerivationAlgorithm
    {
        /// <summary>
        /// The PBKDF2 derivation algorithm.
        /// </summary>
        PBKDF2 = 0
    }

    /// <summary>
    /// The default derivation algorithm.
    /// </summary>
    public static DerivationAlgorithm DefaultAlgorithm = DerivationAlgorithm.PBKDF2;

    /// <summary>
    /// The default parameters for PBKDF2.
    /// </summary>
    public static Dictionary<string, string> DefaultParameters_PBKDF2 = new()
    {
        { "PRF", "HMACSHA512" },
        { "Iterations", "1048576" },
        { "HashLength", "64" }
    };

    /// <summary>
    /// The default parameters for the default derivation algorithm.
    /// </summary>
    public static Dictionary<string, string> DefaultParameters = DefaultParameters_PBKDF2;

    /// <summary>
    /// The length of the salt for newly set passwords.<br/>
    /// Default: 16
    /// </summary>
    public static int DefaultSaltLength = 16;

    /// <summary>
    /// Calculates the hash of the given password using the default hashing algorithm and parameters.<br/>
    /// This is used so a potential attacker can't find out whether an account with a given username exists based on the response time.
    /// </summary>
    public static void WasteTime(string password)
    {
        foreach (byte x in GetHash(password, new byte[DefaultSaltLength], DefaultAlgorithm, DefaultParameters))
        {
            if (x == 0)
            {
                ;
            }
        }
    }

    /// <summary>
    /// The key derivation algorithm to use.
    /// </summary>
    [DataMember]
    private readonly DerivationAlgorithm Algorithm;

    /// <summary>
    /// The parameters for the selected key derivation algorithm.
    /// </summary>
    [DataMember]
    private readonly Dictionary<string, string> Parameters;

    /// <summary>
    /// The salt (adds randomness so two hashes of the same password aren't identical).
    /// </summary>
    [DataMember]
    private readonly byte[] Salt;

    /// <summary>
    /// The hash of the password+salt according to the hashing parameters.
    /// </summary>
    [DataMember]
    private readonly byte[] Hash;

    /// <summary>
    /// Creates a new password hash object for the given password according to the default hashing parameters.
    /// </summary>
    public Password3(string password)
    {
        Algorithm = DefaultAlgorithm;
        Parameters = new();
        foreach (var kv in DefaultParameters)
            Parameters[kv.Key] = kv.Value;
        Salt = RandomNumberGenerator.GetBytes(DefaultSaltLength);
        Hash = GetHash(password);
    }

    /// <summary>
    /// Checks whether the given password matches the one that is saved in this object (at least whether the resulting hash matches).
    /// </summary>
    public bool Check(string password)
        => Hash.SequenceEqual(GetHash(password));

    /// <summary>
    /// Whether the algorithm and parameters match the selected default
    /// </summary>
    public bool MatchesDefault()
    {
        if (Algorithm != DefaultAlgorithm)
            return false;
        if (Salt.Length != DefaultSaltLength)
            return false;
        if (Parameters.Count != DefaultParameters.Count)
            return false;
        if (DefaultParameters.Any(x => (!Parameters.TryGetValue(x.Key, out var v)) || v != x.Value))
            return false;
        return true;
    }

    /// <summary>
    /// Generates the hash for the given password according to the set salt, algorithm and parameters.
    /// </summary>
    private byte[] GetHash(string password)
        => GetHash(password, Salt, Algorithm, Parameters);

    /// <summary>
    /// Generates the hash for the given password according to the given salt, algorithm and parameters.
    /// </summary>
    private static byte[] GetHash(string password, byte[] salt, DerivationAlgorithm algorithm, Dictionary<string,string> parameters)
    {
        switch (algorithm)
        {
            case DerivationAlgorithm.PBKDF2:
                {
                    int iterations = int.Parse(parameters["Iterations"]);
                    int hashLength = int.Parse(parameters["HashLength"]);
                    KeyDerivationPrf prf = parameters["PRF"] switch
                    {
                        "HMACSHA1" => KeyDerivationPrf.HMACSHA1,
                        "HMACSHA256" => KeyDerivationPrf.HMACSHA256,
                        "HMACSHA512" => KeyDerivationPrf.HMACSHA512,
                        _ => throw new NotImplementedException("Unknown base algorithm for PBKDF2.")
                    };
                    return KeyDerivation.Pbkdf2(password, salt, prf, iterations, hashLength);
                }
            default:
                throw new NotImplementedException("Unknown algorithm.");
        }
    }
}
