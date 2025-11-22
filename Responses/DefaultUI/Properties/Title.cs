using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// The page title.
/// </summary>
public class Title : WatchedElement
{
    private readonly RequiredWatchedContainer<MarkdownText> ContentContainer;
    
    private string _Text;
    
    private string? _Suffix;
    
    public Title(string text, string? suffix = null)
    {
        _Text = text;
        _Suffix = suffix;
        ContentContainer = new(this, new(CombinedText));
    }
    
    /// <summary>
    /// The title's text.
    /// </summary>
    public string Text
    {
        get => _Text;
        set
        {
            _Text = value;
            ContentContainer.Element = new(CombinedText);
        }
    }
    
    /// <summary>
    /// The title's suffix.
    /// </summary>
    public string? Suffix
    {
        get => _Suffix;
        set
        {
            _Suffix = value;
            ContentContainer.Element = new(CombinedText);
        }
    }
    
    /// <summary>
    /// The combined text (text and optional suffix).
    /// </summary>
    private string CombinedText
        => _Text + (_Suffix == null ? "" : $" | {_Suffix}");
    
    public override string RenderedTag
        => "title";

    public override IEnumerable<AbstractWatchedContainer?> RenderedContainers
        => [
            ContentContainer
        ];
}