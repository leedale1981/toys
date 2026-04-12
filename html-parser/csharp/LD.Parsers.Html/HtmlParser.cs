using LD.Parsers.Html.Core;

namespace LD.Parsers.Html;

public class HtmlParser
{
    public async Task<HtmlTree> ParseFromText(string text)
    {
        string[] startTags = text.Split('<');
        HtmlTree tree = new();
        
        
        
        
    }
}
