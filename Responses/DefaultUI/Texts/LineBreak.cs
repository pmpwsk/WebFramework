using uwap.WebFramework.Responses.Base;

namespace uwap.WebFramework.Responses.DefaultUI;

/// <summary>
/// A line break.
/// </summary>
public class LineBreak : AbstractElement
{
    public override string RenderedTag
        => "br";
    
    internal static List<AbstractMarkdownPart> Convert(string[] lines)
    {
        List<AbstractMarkdownPart> parts = [];
        foreach (var line in lines)
        {
            if (parts.Count != 0)
                parts.Add(new LineBreak());
            
            bool lineBreak = false;
            foreach (var subline in line.Split('\n'))
            {
                if (lineBreak)
                    parts.Add(new LineBreak());
                parts.Add(new MarkdownText(subline));
                lineBreak = true;
            }
        }
        return parts;
    }
}