namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains data for a mail attachment so it can be generated and sent out later.
/// </summary>
public class MailGenAttachment(string path, string name, string? contentType)
{
    /// <summary>
    /// The file's name.
    /// </summary>
    public string Name = name;

    /// <summary>
    /// The file's content type.
    /// </summary>
    public string? ContentType = contentType;

    /// <summary>
    /// The bytes of the file.
    /// </summary>
    public byte[] Bytes = File.ReadAllBytes(path);
}