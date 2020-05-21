using ProductApi.Models;

namespace ProductApi
{
    public interface IProductParser
    {
        Product ParseProduct(string productUrl);
    }
}
