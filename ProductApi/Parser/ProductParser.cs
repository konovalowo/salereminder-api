using System;
using System.Globalization;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;

using ProductApi.Models;

using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using AngleSharp.Dom;
using System.Text;
using ProductApi.Parser;
using SQLitePCL;

namespace ProductApi
{
    public class ProductParser : IProductParser
    {
        private ISingleProductParser[] parsers;

        public ProductParser(ISingleProductParser[] parsers) 
        {
            this.parsers = parsers;
        }

        public Product ParseProduct(string productUrl)
        {
            IHtmlDocument dom = GetHtml(productUrl);
            foreach (var parser in parsers)
            {
                if (parser.Check(dom))
                {
                    return parser.Parse(productUrl, dom);
                }
            }
            return null;
        }

        private IHtmlDocument GetHtml(string productUrl)
        {
            string htmlRaw;
            byte[] htmlBytes;

            try
            {
                using (WebClient client = new WebClient())
                {
                    //client.Encoding = System.Text.Encoding.GetEncoding("windows-1251");
                    htmlBytes = client.DownloadData(productUrl);
                }
            }
            catch (WebException e)
            {
                throw new ParserException("Unable to download html string: " + e.Message);
            }
            catch (Exception e)
            {
                throw new ParserException("Exception while downloading html string: " + e.Message);
            }

            htmlRaw = Encoding.UTF8.GetString(htmlBytes);
            var dom = new HtmlParser().ParseDocument(htmlRaw);

            string encodingString;
            var encodingCell = dom.QuerySelector("[charset]");
            if (encodingCell == null)
            {
                encodingCell = dom.QuerySelector("[http-equiv=\"Content-Type\"]");
                string encdoingCellContent = encodingCell.GetAttribute("content");
                encodingString = encdoingCellContent.Substring(encdoingCellContent.IndexOf("charset") + 8);
            }
            else if (encodingCell.HasAttribute("cahrset"))
            {
                encodingString = encodingCell.GetAttribute("charset");
            }
            else
            {
                return dom;
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encodingDestination = Encoding.GetEncoding(encodingString);

            htmlRaw = encodingDestination.GetString(htmlBytes);
            return new HtmlParser().ParseDocument(htmlRaw);
        }
    }
}
