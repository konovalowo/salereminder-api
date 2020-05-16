using System;
using System.Collections.Generic;
using System.Text;

using ProductApi.Models;

namespace ProductApi
{
    interface IProductParser
    {
        Product ParseProduct(string productUrl);
    }
}
