using System.Runtime.Serialization;
using uwap.Database;

namespace uwap.WebFramework.Accounts;

/// <summary>
/// Old version of the User class (only properties!) to allow old objects to still be parsed and upgraded.
/// </summary>
[DataContract]
public class User_Old2 : ILegacyTableValue
{
    [DataMember]
    public string Id = "";

    [DataMember]
    public string _Username = "";

    [DataMember]
    public string _MailAddress = "";

    [DataMember]
    public ushort _AccessLevel = 1;

    [DataMember]
    public Password2? Password = null;

    [DataMember]
    public string? MailToken = null;

    [DataMember]
    public DateTime Signup = DateTime.UtcNow;

    [DataMember]
    public TwoFactorData_Old? _TwoFactor = null;

    [DataMember]
    public Dictionary<string, AuthTokenData> _AuthTokens = [];

    [DataMember]
    public Dictionary<string, string> _Settings = [];
}