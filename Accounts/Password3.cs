using Isopoh.Cryptography.Argon2;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;

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
        /// The PBKDF2 derivation algorithm (supported by Microsoft).
        /// </summary>
        PBKDF2 = 0,

        /// <summary>
        /// The Argon2 derivation algorithm (winner of the Password Hashing Competition).
        /// </summary>
        Argon2 = 1,
    }

    /// <summary>
    /// The default derivation algorithm.<br/>
    /// If you change this, you need to change DefaultParameters as well.<br/>
    /// Default: Argon2
    /// </summary>
    public static DerivationAlgorithm DefaultAlgorithm = DerivationAlgorithm.Argon2;

    /// <summary>
    /// The default parameters for PBKDF2.<br/>
    /// PRF=HMACSHA512<br/>
    /// Iterations=1048576<br/>
    /// HashLength=64
    /// </summary>
    public static Dictionary<string, string> DefaultParameters_PBKDF2 = new()
    {
        { "PRF", "HMACSHA512" },
        { "Iterations", "1048576" },
        { "HashLength", "64" }
    };

    /// <summary>
    /// The default parameters for Argon2.<br/>
    /// Type=id<br/>
    /// Version=19<br/>
    /// Time=10<br/>
    /// Memory=32768<br/>
    /// Lanes=8<br/>
    /// HashLength=32
    /// </summary>
    public static Dictionary<string, string> DefaultParameters_Argon2 = new()
    {
        { "Type", "id" },
        { "Version", "19" },
        { "Time", "10" },
        { "Memory", "32768" },
        { "Lanes", "8" },
        { "HashLength", "32" }
    };

    /// <summary>
    /// The default parameters for the default derivation algorithm.<br/>
    /// If you're uncertain whether your current parameters work and how long it takes, execute WasteTime() to find out.<br/>
    /// Default: DefaultParameters_Argon2
    /// </summary>
    public static Dictionary<string, string> DefaultParameters = DefaultParameters_Argon2;

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
    /// Creates a new password hash object for the given password according to the default hashing algorithm and parameters.
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
    internal bool Check(string password)
        => Hash.SequenceEqual(GetHash(password));

    /// <summary>
    /// Whether the algorithm and parameters match the selected default
    /// </summary>
    internal bool MatchesDefault()
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
            case DerivationAlgorithm.Argon2:
                {
                    Argon2Type type = parameters["Type"] switch
                    {
                        "id" => Argon2Type.HybridAddressing,
                        "i" => Argon2Type.DataIndependentAddressing,
                        "d" => Argon2Type.DataDependentAddressing,
                        _ => throw new NotImplementedException("Unknown type for Argon2.")
                    };
                    Argon2Version version = parameters["Version"] switch
                    {
                        "19" => Argon2Version.Nineteen,
                        "16" => Argon2Version.Sixteen,
                        _ => throw new NotImplementedException("Unknown version for Argon2.")
                    };
                    int time = int.Parse(parameters["Time"]);
                    int memory = int.Parse(parameters["Memory"]);
                    int lanes = int.Parse(parameters["Lanes"]);
                    int hashLength = int.Parse(parameters["HashLength"]);
                    Argon2Config config = new()
                    {
                        Type = type,
                        Version = version,
                        TimeCost = time,
                        MemoryCost = memory,
                        Lanes = lanes,
                        Threads = Environment.ProcessorCount,
                        Password = Encoding.UTF8.GetBytes(password),
                        Salt = salt,
                        HashLength = hashLength,
                        ClearPassword = true
                    };
                    using var secureHash = new Argon2(config).Hash();
                    byte[] hash = new byte[secureHash.Buffer.Length];
                    for (int i = 0; i < hash.Length; i++)
                        hash[i] = secureHash[i];
                    secureHash.Dispose();
                    return hash;
                }
            default:
                throw new NotImplementedException("Unknown algorithm.");
        }
    }
}
