using Microsoft.AspNetCore.Http;

namespace uwap.WebFramework.Localization;

/// <summary>
/// An entry for an accepted language in a request.
/// </summary>
public readonly struct LocaleEntry
{
    /// <summary>
    /// Parses all accepted locales in the HTTP context into a list of locale entries.
    /// </summary>
    public static IReadOnlyCollection<LocaleEntry> ParseAll(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue("Accept-Language", out var headerValues))
            return [ ..headerValues
                .SelectMany(value => value == null ? [] : value.Split(','))
                .Select(value => new LocaleEntry(value))
                .Where(locale => Server.Config.AllowedLanguages.Contains(locale.Language)
                                 || Server.Config.AllowedLanguages.Contains(locale.Code))
                .OrderByDescending(locale => locale.Quality) ];
        else
            return [];
    }
    
    /// <summary>
    /// The language's code.
    /// </summary>
    public readonly string Language;

    /// <summary>
    /// The region's code, if one was provided.
    /// </summary>
    public readonly string? Region;
        
    /// <summary>
    /// The language code if no region was provided, or the language and region codes separated with an underscore.
    /// </summary>
    public string Code
        => Region == null ? Language : $"{Language}_{Region}";
        
    /// <summary>
    /// The desirability of the entry.
    /// </summary>
    public readonly double Quality;

    public LocaleEntry(string value)
    {
        if (value.SplitAtFirst(';', out var code, out var qualityString))
        {
            value = code.Trim();
            if (double.TryParse(qualityString.After('=').Trim(), out var quality))
                Quality = quality;
            else
                Quality = 1;
        }
        else
        {
            value = value.Trim();
            Quality = 1;
        }
        
        if (value.SplitAtFirst('-', out var language, out var region))
        {
            Language = language.ToLower();
            Region = region.ToUpper();
        }
        else
        {
            Language = value.ToLower();
            Region = null;
        }
    }
}