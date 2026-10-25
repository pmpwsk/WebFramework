namespace uwap.WebFramework.Localization;

/// <summary>
/// Contains translations for common texts.
/// </summary>
public static class CommonTexts
{
    /// <summary>
    /// Creates a new translation with the given icon and values for english and german.
    /// </summary>
    public static Translation Create(string? icon, string english, string german)
        => new(icon, ("en", english), ("de", german));
}