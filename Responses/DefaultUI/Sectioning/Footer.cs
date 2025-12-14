using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A default UI footer.
/// </summary>
public class Footer : WatchedElement
{
    private string? _CopyrightName;

    private readonly OptionalWatchedContainer<Paragraph> CopyrightContainer;
    
    private readonly RequiredWatchedContainer<Paragraph> PoweredByContainer;
    
    public Footer(string? copyrightName)
    {
        _CopyrightName = copyrightName;
        CopyrightContainer = new(this, _CopyrightName == null ? null : new(BuildCopyright()));
        PoweredByContainer = new(this, new Paragraph([
            new MarkdownText("Powered by "),
            new Link("uwap.org/wf", "https://uwap.org/wf")
        ]));
    }

    /// <summary>
    /// The copyright holder's name.
    /// </summary>
    public string? CopyrightName
    {
        get => _CopyrightName;
        set
        {
            _CopyrightName = value;
            if (value == null)
                CopyrightContainer.Element = null;
            else if (CopyrightContainer.Element != null)
                CopyrightContainer.Element.SetLines(BuildCopyright());
            else
                CopyrightContainer.Element = new(BuildCopyright());
        }
    }

    public override string RenderedTag
        => "footer";

    private string BuildCopyright()
        => $"Copyright {DateTime.UtcNow.Year} {_CopyrightName} - All other trademarks, screenshots, logos and copyrights are the property of their respective owners.";
}