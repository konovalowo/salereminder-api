using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ProductApi.Models;
using System.Collections.Generic;
using System.Globalization;

namespace ProductApi.Parser
{
    public class ProductParserSchemaHtml : ISingleProductParser
    {
        private IElement _schemaProductElement;

        public bool Check(IHtmlDocument dom) => 
            (_schemaProductElement = dom.QuerySelector("[itemtype=\"http://schema.org/Product\"]")) != null;

        public Product Parse(string productUrl, IHtmlDocument dom)
        {
            var productProps = GetProductProperties(dom, _schemaProductElement);

            ValidateProductProperties(productProps, productUrl);

            return new Product
            {
                Url = productUrl,
                Name = productProps.GetValueOrDefault("name", productUrl),
                Brand = productProps.GetValueOrDefault("brand", null),
                Currency = productProps["priceCurrency"],
                Price = double.Parse(productProps["price"], CultureInfo.InvariantCulture),
                IsOnSale = false, // TO DO
                Description = productProps.GetValueOrDefault("description", null),
                Image = productProps.GetValueOrDefault("image", null),
            };
        }

        private Dictionary<string, string> GetProductProperties(IHtmlDocument dom, IElement schemaProductElement)
        {
            var itemPropCells = schemaProductElement?.QuerySelectorAll("[itemprop]") 
                                ?? dom.QuerySelectorAll("[itemprop]");

            var productProps = new Dictionary<string, string>();
            foreach (var itemProp in itemPropCells)
            {
                var key = itemProp.GetAttribute("itemprop");
                if (productProps.ContainsKey(key)) continue;

                var value = itemProp.TextContent;

                if (value == "")
                    value = itemProp.GetAttribute("content");

                if (key == "image" && (value == "" || value == null))
                    value = itemProp.GetAttribute("src");

                value = value?.RemoveExtraWS();

                productProps.Add(key, value);
            }

            return productProps;
        }

        private void ValidateProductProperties(Dictionary<string, string> productProps, string productUrl)
        {
            if (!productProps.TryGetValue("priceCurrency", out var currency))
                throw new ParserException("Can't parse product, price currency not found");

            if (!productProps.ContainsKey("name") || !productProps.ContainsKey("price"))
                throw new ParserException("Can't parse product, name or price not found");

            if (productProps.TryGetValue("image", out var image) 
                && image.StartsWith('/') && !image.StartsWith("//"))
            {
                productProps["image"] = productUrl.GetSecondDomain() + productProps["image"];
            }
        }
    }
}
