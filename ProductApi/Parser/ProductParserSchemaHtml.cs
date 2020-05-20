using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using ProductApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Parser
{
    public class ProductParserSchemaHtml : IProductParser
    {
        private IElement cell;

        public bool Check(IHtmlDocument dom)
        {
            cell = dom.QuerySelector("[itemtype=\"http://schema.org/Product\"]");
            return cell != null;
        }

        public Product Parse(string productUrl, IHtmlDocument dom)
        {
            // Extracting item properties
            var itemPropCells = cell?.QuerySelectorAll("[itemprop]");
            if (cell == null)
            {
                itemPropCells = dom.QuerySelectorAll("[itemprop]");
            }

            // Parse all itemprops to dictionary
            var productProps = new Dictionary<string, string>();
            foreach (var itemProperty in itemPropCells)
            {
                string key = itemProperty.GetAttribute("itemprop");
                if (!productProps.ContainsKey(key))
                {
                    string value = itemProperty.TextContent;

                    if (value == "")
                        value = itemProperty.GetAttribute("content");

                    // Image url parse
                    if (key == "image" && (value == "" || value == null))
                        value = itemProperty.GetAttribute("src");

                    if (value != null)
                        value = value.RemoveExtraWS();

                    productProps.Add(key, value);
                }
            }

            if (productProps["priceCurrency"] == null)
            {
                var currencyCell = dom.QuerySelector("[itemprop=\"priceCurrency\"]");
                if (currencyCell == null)
                {
                    throw new ParserException("Can't parse product, price currency not found");
                }
            }

            if (productProps["name"] == null ||
                productProps["price"] == null)
            {
                throw new ParserException("Can't parse product, name or price not found");
            }

            if (productProps.ContainsKey("image") && productProps["image"].StartsWith('/')
                && !productProps["image"].StartsWith("//"))
            {
                productProps["image"] = productUrl.GetSecondDomain() + productProps["image"];
            }

            Product product = new Product
            {
                Url = productUrl,
                Name = productProps.ContainsKey("name") ? productProps["name"] : productUrl,
                Brand = productProps.ContainsKey("brand") ? productProps["brand"] : null,
                Currency = productProps["priceCurrency"],
                Price = double.Parse(productProps["price"], CultureInfo.InvariantCulture),
                IsOnSale = false, // TO DO
                Description = productProps.ContainsKey("description") ? productProps["description"] : null,
                Image = productProps.ContainsKey("image") ? productProps["image"] : null,
            };

            return product;
        }
    }
}
