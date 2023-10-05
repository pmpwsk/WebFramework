using MimeKit;

namespace uwap.WebFramework.Mail;

/// <summary>
/// Contains data for a mail attachment so it can be generated and sent out later.
/// </summary>
public class MailGenAttachment
{
    /// <summary>
    /// The file's name.
    /// </summary>
    public string? Name;

    /// <summary>
    /// The file's content type.
    /// </summary>
    public string? ContentType;

    /// <summary>
    /// The bytes of the file.
    /// </summary>
    public byte[] Bytes;

    /// <summary>
    /// Creates a new mail attachment object.
    /// </summary>
    public MailGenAttachment(string path, string? name, string? contentType)
    {
        Bytes = File.ReadAllBytes(path);
        Name = name;
        ContentType = contentType;
    }
}