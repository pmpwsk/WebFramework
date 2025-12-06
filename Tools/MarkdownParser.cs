using System.Text;
using uwap.WebFramework.Responses;
using uwap.WebFramework.Responses.Base;
using uwap.WebFramework.Responses.DefaultUI;
using uwap.WebFramework.Responses.Dynamic;

namespace uwap.WebFramework;

/// <summary>
/// Parses .wfmd files into default UI pages.
/// </summary>
public static class MarkdownParser
{
    /// <summary>
    /// Attempts to handle the given request using a .wfmd file in ../Public for any of the domains (in order) and returns true if that was possible. If no matching file was found, false is returned.
    /// </summary>
    public static IResponse? HandleRequest(Request req, List<string> domains)
    {
        string path = req.Path;
        if (path.EndsWith("/index"))
            return null;
        if (path.EndsWith('/'))
            path += "index";
        path += ".wfmd";
        
        foreach (string domain in domains)
            if (Server.Cache.TryGetValue(domain + path, out Server.CacheEntry? entry) && entry.IsPublic)
            {
                var page = new Page(req, false)
                {
                    Title = entry.Key.After('/').RemoveLast(5).CapitalizeFirstLetter()
                };
                Apply(req, page, entry.EnumerateTextLines());
                return page;
            }
        
        return null;
    }
    
    private class State
    {
        public readonly Page Page;
        public Section? Section;
        public AbstractSubsection? Subsection;
        public List<int> ListIndentations = [0];
    
        public State(Page page)
        {
            Page = page;
            Section = page.Sections.EnumerateTyped().LastOrDefault();
            Subsection = Section?.Subsections.EnumerateTyped().LastOrDefault();
        }
    }
    
    private interface ILine
    {
        public void ApplyTo(Request req, State state);
    }
    
    private interface IAppendableLine : ILine
    {
        public void Append(string line);
    }
    
    private interface IRawReaderLine : ILine
    {
        public bool Append(string line);
    }
    
    private class EmptyLine : ILine
    {
        public void ApplyTo(Request req, State state)
        {
        }
    }
    
    private class CommandLine(string command) : ILine
    {
        public readonly string Command = command;
        
        public void ApplyTo(Request req, State state)
        {
            if (!Command.SplitAtFirst(' ', out var name, out string? value) || string.IsNullOrWhiteSpace(value))
                value = null;
            
            switch (name.ToLower())
            {
                case "title":
                    if (value != null)
                        state.Page.Title = value;
                    break;
                case "description":
                    if (value != null)
                        state.Page.Description = value;
                    break;
            }
        }
    }
    
    private class SectionLine(string title) : ILine
    {
        public readonly string Title = title;
        
        public void ApplyTo(Request req, State state)
        {
            state.Section = new(Title);
            state.Subsection = new Subsection(null);
            state.Section.Subsections.Add(state.Subsection);
            state.Page.Sections.Add(state.Section);
        }
    }
    
    private class SubsectionLine(string title) : ILine
    {
        public readonly string Title = title;
        
        public void ApplyTo(Request req, State state)
        {
            if (state.Section == null)
                return;
        
            if (state.Subsection is { Header: null } && !state.Subsection.Content.Any())
                state.Subsection.Header = new(Title);
            else
            {
                state.Subsection = new Subsection(Title);
                state.Section.Subsections.Add(state.Subsection);
            }
        }
    }
    
    private class HeadingLine(string title) : ILine
    {
        public readonly string Title = title;
        
        public void ApplyTo(Request req, State state)
            => state.Subsection?.Content.Add(new Heading3(Title));
    }
    
    private class ImageLine(string name, string url) : ILine
    {
        public readonly string Name = name;
        public readonly string Url = url;

        public void ApplyTo(Request req, State state)
            => state.Subsection?.Content.Add(new Image(req, Url, Name));
    }
    
    private class CodeBlockLine : IRawReaderLine
    {
        public List<string> Lines = [];
        
        public bool Append(string line)
        {
            if (line.Trim() == "```")
            {
                return true;
            }
            else
            {
                Lines.Add(line);
                return false;
            }
        }
        
        public void ApplyTo(Request req, State state)
            => state.Subsection?.Content.Add(new CodeBlock(Lines.ToArray()));
    }
    
    private class TextLine(string text) : IAppendableLine
    {
        public string Text = text.Trim();
        
        public void Append(string line)
            => Text = $"{Text} {line.Trim()}";
        
        public void ApplyTo(Request req, State state)
            => state.Subsection?.Content.Add(new Paragraph(ReadText(Text)));
    }
    
    private abstract class AbstractListItemLine(IListItemType itemType, int indentation) : ILine
    {
        protected readonly IListItemType ItemType = itemType;
        
        protected readonly int Indentation = indentation;
        
        protected abstract IEnumerable<AbstractMarkdownPart> Generate(Request req);
        
        public void ApplyTo(Request req, State state)
        {
            if (state.Subsection == null)
                return;
            
            var newItem = new ListItem(Generate(req));
            
            int index = 0;
            IListWatchedContainer<AbstractList> parent = state.Subsection.Content;
            foreach (var levelIndentation in (IEnumerable<int>)[..state.ListIndentations, Indentation])
            {
                if (parent.LastOrDefault() is not AbstractList abstractList)
                {
                    parent.Add(ItemType.CreateList([newItem]));
                    state.ListIndentations.ReplaceEnd(index, Indentation);
                    return;
                }
                
                if (Indentation <= levelIndentation)
                {
                    state.ListIndentations.ReplaceEnd(index, Indentation);
                    if (ItemType.MatchesList(abstractList))
                        abstractList.Items.Add(newItem);
                    else
                        parent.Add(ItemType.CreateList([newItem]));
                    return;
                }
                
                var lastItem = abstractList.Items.EnumerateTyped().LastOrDefault();
                if (lastItem == null)
                {
                    parent.Add(ItemType.CreateList([newItem]));
                    state.ListIndentations.ReplaceEnd(index, Indentation);
                    return;
                }
                
                parent = lastItem.Sublists;
                index++;
            }
        }
    }
    
    private class ImageListItemLine(IListItemType itemType, int indentation, string name, string url)
        : AbstractListItemLine(itemType, indentation)
    {
        public readonly string Name = name;
        public readonly string Url = url;
        
        protected override IEnumerable<AbstractMarkdownPart> Generate(Request req)
            => [new Image(req, Url, Name)];
    }
    
    private class TextListItemLine(IListItemType itemType, int indentation, string text)
        : AbstractListItemLine(itemType, indentation), IAppendableLine
    {
        public string Text = text.Trim();
        
        public void Append(string line)
            => Text = $"{Text} {line.Trim()}";
        
        protected override IEnumerable<AbstractMarkdownPart> Generate(Request req)
            => ReadText(Text);
    }
    
    private class CodeListItemLine(IListItemType itemType, int indentation, int codeIndentation) : AbstractListItemLine(itemType, indentation), IRawReaderLine
    {
        public readonly int CodeIndentation = codeIndentation;
        public List<string> Lines = [];
        
        public bool Append(string line)
        {
            if (line.Trim() == "```")
                return true;
            
            line = line.Replace("\t", "    ");
            var start = Math.Min(line.MeasureIndentation().Indentation, CodeIndentation);
            line = line[start..];
            Lines.Add(line);
            return false;
        }
        
        protected override IEnumerable<AbstractMarkdownPart> Generate(Request req)
            => [new CodeBlock(Lines.ToArray())];
    }
    
    private interface IListItemType
    {
        public AbstractList CreateList(IEnumerable<ListItem> items);
        
        public bool MatchesList(AbstractList list);
    }
    
    private class BulletItemType : IListItemType
    {
        public AbstractList CreateList(IEnumerable<ListItem> items)
            => new BulletList(items);
        
        public bool MatchesList(AbstractList list)
            => list is BulletList;
    }
    
    private class OrderedItemType(ListMarkerType type) : IListItemType
    {
        public readonly ListMarkerType Type = type;
        
        public AbstractList CreateList(IEnumerable<ListItem> items)
            => new OrderedList(Type, items);
        
        public bool MatchesList(AbstractList list)
            => list is OrderedList orderedList && orderedList.Type == OrderedList.ConvertType(Type);
    }

    /// <summary>
    /// Parses the .wfmd file in the given lines and populates the given page with it.
    /// </summary>
    public static void Apply(Request req, Page page, IEnumerable<string> lines)
    {
        var state = new State(page);
        foreach (var line in ProcessAll(lines))
            line.ApplyTo(req, state);
    }
    
    private static IEnumerable<ILine> ProcessAll(IEnumerable<string> lines)
    {
        ILine? previous = null;
        foreach (var unescapedLine in lines)
        {
            if (previous is IRawReaderLine rawReaderLine)
            {
                if (rawReaderLine.Append(unescapedLine))
                {
                    yield return previous;
                    previous = null;
                }
                continue;
            }
            
            var line = EscapeText(unescapedLine);
            var current = Process(line);
            if (previous is IAppendableLine appendableLine && current is TextLine)
            {
                appendableLine.Append(line);
                continue;
            }
            
            if (previous != null)
            {
                yield return previous;
                previous = null;
            }
            
            switch (current)
            {
                case IAppendableLine currentAppendable:
                    previous = currentAppendable;
                    break;
                case IRawReaderLine currentRawReader:
                    previous = currentRawReader;
                    break;
                default:
                    yield return current;
                    break;
            }
        }
        
        if (previous != null)
            yield return previous;
    }
    
    private static ILine Process(string rawLine)
    {
        var (indentation, line) = rawLine.MeasureIndentation();

        if (string.IsNullOrEmpty(line))
            return new EmptyLine();
        else if (line.StartsWith("|", out var rest))
            return new CommandLine(rest);
        else if (line.StartsWith("# ", out rest))
            return new SectionLine(rest);
        else if (line.StartsWith("## ", out rest))
            return new SubsectionLine(rest);
        else if (line.StartsWith("### ", out rest))
            return new HeadingLine(rest);
        else if (line.StartsWith("-", out rest))
            return ProcessListItem(new BulletItemType(), indentation, rest);
        else if (line.StartsWith("1.", out rest))
            return ProcessListItem(new OrderedItemType(ListMarkerType.Numbers), indentation, rest);
        else if (line.StartsWith("a.", out rest))
            return ProcessListItem(new OrderedItemType(ListMarkerType.LettersLowercase), indentation, rest);
        else if (line.StartsWith("A.", out rest))
            return ProcessListItem(new OrderedItemType(ListMarkerType.LettersUppercase), indentation, rest);
        else if (line.StartsWith("i.", out rest))
            return ProcessListItem(new OrderedItemType(ListMarkerType.RomanNumbersLowercase), indentation, rest);
        else if (line.StartsWith("I.", out rest))
            return ProcessListItem(new OrderedItemType(ListMarkerType.RomanNumbersUppercase), indentation, rest);
        else if (line.Trim().IsMarkedSegment("![", "](", ")", out var name, out var url))
            return new ImageLine(name, url);
        else if (line.Trim() == "```")
            return new CodeBlockLine();
        else
            return new TextLine(line);
    }
    
    private static ILine ProcessListItem(IListItemType type, int indentation, string content)
    {
        if (content.Trim().IsMarkedSegment("![", "](", ")", out var name, out var url))
            return new ImageListItemLine(type, indentation, name, url);
        else if (content.Trim() == "```")
            return new CodeListItemLine(type, indentation, indentation + (type is BulletItemType ? 1 : 2) + content.MeasureIndentation().Indentation);
        else
            return new TextListItemLine(type, indentation, content.TrimStart());
    }
    
    private static string EscapeText(string text)
    {
        var sb = new StringBuilder();
        bool escape = false;
        foreach (var c in text)
            if (escape)
            {
                sb.Append(c switch
                {
                    '\\' => "&#92;",
                    '`' => "&#96;",
                    '*' => "&#42;",
                    '_' => "&#95;",
                    '{' => "&#123;",
                    '}' => "&#125;",
                    '[' => "&#91;",
                    ']' => "&#93;",
                    '<' => "&#60;",
                    '>' => "&#62;",
                    '(' => "&#40;",
                    ')' => "&#41;",
                    '#' => "&#35;",
                    '+' => "&#43;",
                    '-' => "&#45;",
                    '.' => "&#46;",
                    '!' => "&#33;",
                    '|' => "&#124;",
                    _ => c.ToString()
                });
                escape = false;
            }
            else if (c == '\\')
                escape = true;
            else
                sb.Append(c);
        
        return sb.ToString();
    }
    
    private static List<AbstractMarkdownPart> ReadText(string text)
    {
        List<AbstractMarkdownPart> result = [];
        Stack<(string Symbol, List<AbstractMarkdownPart> Buffer)> stack = [];
        
        string read = "";
        foreach (var c in text)
        {
            read += c;
            if (stack.TryPeek(out var top) && read.EndsWith(top.Symbol, out var rest))
            {
                read = "";
                Text(rest);
                Pop();
            }
            else if (read.EndsWithAny(out rest, out var end, "`", "**", "__"))
            {
                read = "";
                Text(rest);
                stack.Push((end, []));
            }
        }
        
        Text(read);
        while (stack.Count > 0)
            Pop();
        
        return result;
        
        void Output(AbstractMarkdownPart part)
        {
            if (stack.TryPeek(out var newTop))
                newTop.Buffer.Add(part);
            else
                result.Add(part);
        }
        
        void Pop()
        {
            var top = stack.Pop();
            AbstractMarkdownPart part = top.Symbol switch
            {
                "`" => new CodeSegment(top.Buffer),
                "**" => new BoldText(top.Buffer),
                "__" => new ItalicsText(top.Buffer),
                _ => throw new Exception("Finished unknown markdown segment.")
            };
            Output(part);
        }
        
        void Text(string value)
        {
            if (value == "")
                return;
            
            while (value.ContainsMarkedSegment("[", "](", ")", out var before, out var name, out var url, out var after))
            {
                Output(new MarkdownText(before));
                Output(new Link(name, url));
                value = after;
            }
            Output(new MarkdownText(value));
        }
    }
}