﻿using ProductApi.Models;
using ProductApi.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public class ParserService : IParserService
    {
        private readonly ISchemaProductParser _parser;

        public ParserService(IProductParser[] parsers)
        {
            _parser = new SchemaProductParser(parsers);
        }

        public async Task<Product> Parse(string productUrl)
        {
            return await Task.Run(() => _parser.ParseProduct(productUrl));
        }
    }
}
