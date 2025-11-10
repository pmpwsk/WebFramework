namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains data for a mail attachment so it can be generated and sent out later.
/// </summary>
public class MailGenAttachment(string name, string? contentType, byte[] bytes)
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
    public byte[] Bytes = bytes;
}