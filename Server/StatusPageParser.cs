using uwap.WebFramework.Elements;

namespace uwap.WebFramework;

public static partial class Server
{
    internal static IEnumerable<string> ParseStatusPageAndReturnExport(AppRequest request, CacheEntry cacheEntry, string message)
    {
        Presets.CreatePage(request, cacheEntry.Key.After('/').RemoveLast(5).CapitalizeFirstLetter(), out Page page, out _);
        Presets.Navigation(request, page);
        ParsePage(request, page, cacheEntry);
        if (page.Description == "")
            page.Description = null;

        ReplaceN(ref page.Description);
        ReplaceR(ref page.Title);
        foreach (var e in (IEnumerable<IPageElement>)[..page.Elements, ..page.Sidebar])
            if (e is IContainerElement container) 
            {
                ReplaceN(ref container.Title);
                foreach (var c in container.Contents)
                    switch (c)
                    {
                        case BulletList bl:
                            bl.List = bl.List.Select(Replace).ToList();
                            break;
                        case OrderedList ol:
                            ol.List = ol.List.Select(Replace).ToList();
                            break;
                        case Paragraph p:
                            ReplaceR(ref p.Text);
                            break;
                        case Heading h:
                            ReplaceR(ref h.Text);
                            break;
                    }
            }

        return page.ExportWithoutCheckingForError(request);

        string Replace(string v)
            => v.Replace("[STATUS]", request.Status.ToString())
                .Replace("[MESSAGE]", message);

        void ReplaceR(ref string v)
            => v = Replace(v);

        void ReplaceN(ref string? v)
        {
            if (v != null)
                ReplaceR(ref v);
        }
    }
}