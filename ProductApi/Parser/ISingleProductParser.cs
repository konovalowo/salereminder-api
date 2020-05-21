using AngleSharp.Html.Dom;
using ProductApi.Models;

namespace ProductApi.Parser
{
    public interface ISingleProductParser
    {
        bool Check(IHtmlDocument dom);

        Product Parse(string url, IHtmlDocument dom);
    }
}
