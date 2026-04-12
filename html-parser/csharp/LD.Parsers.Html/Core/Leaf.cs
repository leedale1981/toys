using System.Security.AccessControl;
using LD.Parsers.Html.Models;

namespace LD.Parsers.Html.Core;

public class Leaf
{
    public int Order { get; set; }
    public HtmlTag HtmlTag { get; set; }
    public List<Leaf> Leaves { get; set; }
}