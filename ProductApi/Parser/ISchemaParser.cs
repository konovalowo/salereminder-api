using System;
using System.Collections.Generic;
using System.Text;

using ProductApi.Models;

namespace ProductApi
{
    interface ISchemaParser
    {
        string ProductUrl { get; set; }

        void SetHtml(string prodUrl);

        Product ParseProduct();
    }
}
