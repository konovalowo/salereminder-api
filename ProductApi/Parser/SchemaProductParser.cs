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
                case "json":
                    return ParseProductJSON(productUrl, dom);
                case "xml":
                    return ParseProductXML(productUrl, dom);
                default:
                    return null;
            }
        }

        private string CheckDataType(IHtmlDocument dom)
        {
            if (ExtractProductJson(dom) != null)
            {
                return "json";
            }
            else if (dom.QuerySelectorAll("[itemprop]") != null)
            {
                return "xml";
            }
            else
            {
                return null;
            }
        }

        private IHtmlDocument GetHtml(string productUrl)
        {
            string rawHtml;
            using (WebClient client = new WebClient())
            {
                client.Encoding = System.Text.Encoding.UTF8;
                //Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                //client.Encoding = System.Text.Encoding.GetEncoding("windows-1251");
                rawHtml = client.DownloadString(productUrl);
            }

            // Creating DOM
            return new HtmlParser().ParseDocument(rawHtml);
        }

        private Product ParseProductJSON(string productUrl, IHtmlDocument dom)
        {

            JObject productJson = ExtractProductJson(dom);

            string brand = ParseBrandProp(productJson);
            var offerToken = productJson["offers"];
            string currency = ParseCurrencyProp(offerToken);
            decimal price = ParsePriceProp(offerToken);
            string image = ParseImageProp(productJson);

            Product product = new Product
            {
                Url = productUrl,
                //Id = (string)productJson["sku"],
                Name = (string)productJson["name"],
                Brand = brand,
                Currency = currency,
                Price = price,
                Discount = 0,
                Description = (string)productJson["description"],
                Image = image,
            };

            return product;
        }

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
                    if (key == "image" && key == "")
                        value = itemProperty.GetAttribute("src");

                    if (value != null)
                        value = value.RemoveExtraWS();

                    productProps.Add(key, value);
                }
            }

            // Product id
            if (!productProps.ContainsKey("sku") || productProps["sku"] == "")
            {
                productProps["sku"] = null;
            }

            Product product = new Product
            {
                Url = productUrl,
                //Id = productProps["sku"],
                Name = productProps["name"],
                Brand = productProps["brand"],
                Currency = productProps["priceCurrency"],
                Price = decimal.Parse(productProps["price"], CultureInfo.InvariantCulture),
                Discount = 0,
                Description = productProps["description"],
                Image = productProps["image"],
            };

            return product;
        }

        private static string ParseBrandProp(JObject productJson)
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

        private static string ParseCurrencyProp(JToken offerToken)
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
                throw new FormatException("Couldn't parse currency");
            }

            return currency;
        }

        private static decimal ParsePriceProp(JToken offerToken)
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
                throw new FormatException("Couldn't parse item price");
            }

            char whitespace = Array.Find(priceString.ToCharArray(), ch => !char.IsDigit(ch));
            priceString = string.Join("", priceString.Split(whitespace));

            price = decimal.Parse(priceString, CultureInfo.InvariantCulture);

            return price;
        }

        private static string ParseImageProp(JObject productJson)
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
    }
}
