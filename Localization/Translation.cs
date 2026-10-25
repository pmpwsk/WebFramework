using uwap.WebFramework.Responses.DefaultUI;
using uwap.WebFramework.Tools;

namespace uwap.WebFramework.Localization;

/// <summary>
/// Contains translations of a specific text.
/// </summary>
public class Translation
{
    /// <summary>
    /// The CSS class list of the icon to use with this text, or null if no icon should be displayed.
    /// </summary>
    public readonly string? Icon;
    
    /// <summary>
    /// The dictionary of locale keys (language code or combined code) and their translation.
    /// </summary>
    public readonly Dictionary<string, string> Translations;

    /// <summary>
    /// Contains translations of a specific text.
    /// </summary>
    public Translation(string? icon, params (string key, string translation)[] translations)
    {
        Icon = icon;
        Translations = [];
        foreach (var (key, translation) in translations)
            Translations[key] = translation;
    }

    public override string ToString()
    {
        if (Dependencies.TryGet<Request>(out var req))
            foreach (var locale in req.Locales)
                if (Translations.TryGetValue(locale.Code, out var translation)
                    || Translations.TryGetValue(locale.Language, out translation))
                    return translation;
        
        return Translations.GetValueOrDefault(Server.Config.DefaultLanguage, "[Invalid translation]");
    }
    
    public static implicit operator IconAndText(Translation text)
        => new(text.Icon, text.ToString());
    
    public static implicit operator string(Translation text)
        => text.ToString();
}