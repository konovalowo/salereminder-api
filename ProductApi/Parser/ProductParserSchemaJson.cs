using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Newtonsoft.Json.Linq;
using ProductApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Parser
{
    public class ProductParserSchemaJson : ISingleProductParser
    {
        private JObject jsonObject;

        public bool Check(IHtmlDocument dom)
        {
            jsonObject = ExtractProductJson(dom);
            return jsonObject != null;
        }

        public Product Parse(string productUrl, IHtmlDocument dom)
        {
            string brand = ParseBrandPropJson(jsonObject);
            var offerToken = jsonObject["offers"];
            string currency = ParseCurrencyPropJson(offerToken);
            double price = ParsePricePropJson(offerToken);
            string image = ParseImagePropJson(jsonObject);

            Product product = new Product
            {
                Url = productUrl,
                Name = (string)jsonObject["name"],
                Brand = brand,
                Currency = currency,
                Price = price,
                IsOnSale = false,  // TO DO: Определение скидки
                Description = (string)jsonObject["description"],
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

        private static double ParsePricePropJson(JToken offerToken)
        {
            double price; //, discount
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

            try
            {
                price = double.Parse(priceString, CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new ParserException("Unable to parse price due to incorrect format");
            }
            catch (Exception e)
            {
                throw new ParserException("Unable to parse price");
            }
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
    }
}
