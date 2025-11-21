namespace uwap.WebFramework.Database;

/// <summary>
/// Contains a byte array, along with an MD5 hash to check its integrity. 
/// </summary>
public class RawFileContent(byte[] data)
{
    /// <summary>
    /// The stored bytes.
    /// </summary>
    public byte[] Data = data;
    
    /// <summary>
    /// The MD5 hash of the stored bytes.
    /// </summary>
    public byte[] Hash = data.ToMD5();
}