namespace uwap.WebFramework.Elements;

public class Preload
{
    public string Link;
    public string As;
    public string? Type;
    public bool Crossorigin;

    public Preload(string link, string loadAs, string? type = null, bool crossorigin = true)
    {
        Link = link;
        As = loadAs;
        Type = type;
        Crossorigin = crossorigin;

        if (Type == null)
        {
            string extension = link;
            if (extension.Contains('/')) extension = extension.Remove(0, extension.LastIndexOf('/')+1);
            if (extension.Contains('.')) extension = extension.Remove(0, extension.LastIndexOf('.'));
            else extension = "";
            if (Server.Config.MimeTypes.ContainsKey(extension)) Type = Server.Config.MimeTypes[extension];
        }
    }

    public string Export()
        => $"<link rel=\"preload\" href=\"{Link}\"{(As==null?"":$" as=\"{As}\"")}{(Type==null?"":$" type=\"{Type}\"")}{(Crossorigin?" crossorigin":"")} />";
}