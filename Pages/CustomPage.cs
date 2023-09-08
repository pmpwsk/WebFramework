using static System.Runtime.InteropServices.JavaScript.JSType;

namespace uwap.WebFramework.Elements;

public class CustomPage : IPage
{
    public List<string> Head = new();
    public List<string> Body = new();

    public CustomPage()
    {
    }

    public override string Export(AppRequest request)
    {
        string page = "";
        page += "<!DOCTYPE html>";
        page += "\n<html>";
        page += "\n<head>";
        //title
        page += $"\n\t<title>{Title}</title>";
        //viewport settings + charset
        page += $"\n\t<meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\" />";
        page += "\n\t<meta charset=\"utf-8\">";
        //favicon
        if (Server.Cache.ContainsKey("any/favicon.ico"))
            page += $"\n\t<link rel=\"icon\" type=\"image/x-icon\" href=\"/favicon.ico?t={Server.Cache["any/favicon.ico"].GetModifiedUtc().Ticks}\">";
        else if (Server.Cache.ContainsKey(request.Domain + "/favicon.ico"))
            page += $"\n\t<link rel=\"icon\" type=\"image/x-icon\" href=\"/favicon.ico?t={Server.Cache[request.Domain + "/favicon.ico"].GetModifiedUtc().Ticks}\">";
        /*//preloads
        foreach (Preload preload in Preloads)
            page += "\n\t" + preload.Export();*/
        //styles
        foreach (IStyle style in Styles)
            foreach (string line in style.Export(request))
                page += "\n\t" + line;
        foreach (string item in Head)
            page += "\n\t" + item;
        page += "\n</head>";

        page += "\n<body>";
        foreach (string item in Body)
            page += "\n\t" + item;
        foreach (IScript script in Scripts)
            foreach (string line in script.Export(request))
                page += "\n\t" + line;

        page += "\n</body>";
        page += "\n</html>";
        return page;
    }
}