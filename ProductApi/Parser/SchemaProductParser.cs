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

namespace ProductApi
{
    public class SchemaProductParser
    {

        public SchemaProductParser() { }

        public Product ParseProduct(string productUrl)
        {
            IHtmlDocument dom = GetHtml(productUrl);
            switch (CheckDataType(dom))
            {
                case MetaType.JSON:
                    return ParseProductJSON(productUrl, dom);
                case MetaType.XML:
                    return ParseProductXML(productUrl, dom);
                default:
                    return null;
            }
        }

        private MetaType CheckDataType(IHtmlDocument dom)
        {
            if (ExtractProductJson(dom) != null)
            {
                return MetaType.JSON;
            }
            else if (dom.QuerySelector("[itemtype=\"http://schema.org/Product\"]") != null)
            {
                return MetaType.XML;
            }
            else
            {
                throw new ParserException("No metadata found");
            }
        }

        private IHtmlDocument GetHtml(string productUrl)
        {
            string htmlRaw;
            byte[] htmlBytes;
            using (WebClient client = new WebClient())
            {
                //client.Encoding = System.Text.Encoding.GetEncoding("windows-1251");
                htmlBytes = client.DownloadData(productUrl);
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
            else
            {
                encodingString = encodingCell.GetAttribute("charset");
            }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding encodingDestination = Encoding.GetEncoding(encodingString);

            htmlRaw = encodingDestination.GetString(htmlBytes);
            return new HtmlParser().ParseDocument(htmlRaw);
        }

        #region XML parsing

        private Product ParseProductXML(string productUrl, IHtmlDocument dom)
        {
            // Extracting item properties
            var productCell = dom.QuerySelector("[itemtype=\"http://schema.org/Product\"]");
            var itemPropCells = productCell?.QuerySelectorAll("[itemprop]");
            if (productCell == null)
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

            if (productProps["img"].StartsWith('/') && !productProps["img"].StartsWith("//"))
            {
                productProps["img"] = productUrl.GetSecondDomain() + productProps["img"];
            }

            Product product = new Product
            {
                Url = productUrl,
                Name = productProps["name"],
                Brand = productProps.ContainsKey("brand") ? productProps["brand"] : null,
                Currency = productProps["priceCurrency"],
                Price = decimal.Parse(productProps["price"], CultureInfo.InvariantCulture),
                IsOnSale = false, // TO DO
                Description = productProps.ContainsKey("description") ? productProps["description"] : null,
                Image = productProps.ContainsKey("image") ? productProps["image"] : null,
            };

            return product;
        }
        #endregion

        #region Json parsing

        private Product ParseProductJSON(string productUrl, IHtmlDocument dom)
        {

            JObject productJson = ExtractProductJson(dom);

            string brand = ParseBrandPropJson(productJson);
            var offerToken = productJson["offers"];
            string currency = ParseCurrencyPropJson(offerToken);
            decimal price = ParsePricePropJson(offerToken);
            string image = ParseImagePropJson(productJson);

            Product product = new Product
            {
                Url = productUrl,
                Name = (string)productJson["name"],
                Brand = brand,
                Currency = currency,
                Price = price,
                IsOnSale = false,  // TO DO: Определение скидки
                Description = (string)productJson["description"],
                Image = image,
            };

            return product;
        }

        private static string ParseBrandPropJson(JObject productJson)
        {
            string brand;
            var brandToken = productJson["brand"];
            if (brandToken != null && brandToken.HasValues)
            {
                brand = (string)brandToken["name"];
            }
            else if (brandToken != null)
            {
                brand = (string)brandToken;
            }
            else
            {
                brand = null;
            }

            return brand;
        }

        private static string ParseCurrencyPropJson(JToken offerToken)
        {
            string currency;
            if (offerToken["priceCurrency"] != null)
            {
                currency = (string)offerToken["priceCurrency"];
            }
            else if (offerToken["PriceCurrency"] != null)
            {
                currency = (string)offerToken["PriceCurrency"];
            }
            else
            {
                throw new ParserException("Couldn't parse currency");
            }

            return currency;
        }

        private static decimal ParsePricePropJson(JToken offerToken)
        {
            decimal price; //, discount
            string priceString = null;
            string[] possiblePriceTags = { "Price", "price", "lowPrice" };
            foreach (string tag in possiblePriceTags)
            {
                if (offerToken[tag] != null)
                {
                    priceString = (string)offerToken[tag];
                    break;
                }
            }
            if (priceString == null)
            {
                throw new ParserException("Couldn't parse item price");
            }

            char whitespace = Array.Find(priceString.ToCharArray(), ch => !char.IsDigit(ch));
            priceString = string.Join("", priceString.Split(whitespace));

            price = decimal.Parse(priceString, CultureInfo.InvariantCulture);

            return price;
        }

        private static string ParseImagePropJson(JObject productJson)
        {
            string image;
            var imageToken = productJson["image"];
            if (imageToken != null && imageToken.HasValues)
            {
                image = (string)imageToken[0];
            }
            else if (imageToken != null)
            {
                image = (string)imageToken;
            }
            else
            {
                image = null;
            }
            return image;
        }

        private JObject ExtractProductJson(IHtmlDocument dom)
        {
            // Extracting item properties
            string productJsonString = null;
            var productJsonCells = dom.QuerySelectorAll("[type=\"application/ld+json\"]");
            foreach (var cell in productJsonCells)
            {
                var json = JObject.Parse(cell.TextContent);
                if ((string)json["@type"] == "Product")
                {
                    productJsonString = cell.TextContent;
                    break;
                }
            }

            if (productJsonString == null)
            {
                return null;
            }

            JObject productJson = JObject.Parse(productJsonString);
            return productJson;
        }
        #endregion

        enum MetaType
        {
            XML,
            JSON
        }
    }
}
