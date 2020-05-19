using ProductApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public class ParserService : IParserService
    {
        private readonly IProductParser _parser;

        public ParserService()
        {
            _parser = new SchemaProductParser();
        }

        public async Task<Product> Parse(string productUrl)
        {
            return await Task.Run(() => _parser.ParseProduct(productUrl));
        }
    }
}
