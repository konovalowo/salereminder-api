using AngleSharp.Html.Dom;
using ProductApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Parser
{
    public interface ISingleProductParser
    {
        bool Check(IHtmlDocument dom);

        Product Parse(string url, IHtmlDocument dom);
    }
}
