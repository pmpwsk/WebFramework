namespace uwap.WebFramework.Tools;

/// <summary>
/// Contains versions of a specific piece of text for different locales.
/// </summary>
public class Translation(string defaultKey, Dictionary<string, string> translations)
{
    public string DefaultKey = defaultKey;

    public Dictionary<string, string> Translations = translations;
    
    public override string ToString()
    {
        var context = Server.CurrentHttpContext;
        if (context != null && context.Request.Headers.TryGetValue("Accept-Language", out var headerValues))
        {
            var locales = headerValues.SelectMany(x => x == null ? [] : x.Split(','))
                    .Select(x => x.SplitAtFirst(';', out var code, out var qualityString) && double.TryParse(qualityString.After('=').Trim(), out var quality)
                            ? new LocaleEntry(code.Trim(), quality)
                            : new(x.Trim(), 1))
                    .OrderByDescending(x => x.Quality).ToList();

            foreach (var locale in locales)
            {
                if (Translations.TryGetValue(locale.Code, out var translation))
                    return translation;
            }

            foreach (var locale in locales)
            {
                if (Translations.TryGetValue(locale.Language, out var translation))
                    return translation;
            }
        }
        
        return Translations.GetValueOrDefault(DefaultKey, "[Invalid translation]");
    }

    private class LocaleEntry
    {
        public readonly string Language;

        public readonly string? Region;
        
        public string Code => Region == null ? Language : $"{Language}_{Region}";
        
        public readonly double Quality;

        public LocaleEntry(string code, double quality)
        {
            if (code.SplitAtFirst('-', out var language, out var region))
            {
                Language = language.ToLower();
                Region = region.ToUpper();
            }
            else
            {
                Language = code.ToLower();
                Region = null;
            }
            
            Quality = quality;
        }
    }
}