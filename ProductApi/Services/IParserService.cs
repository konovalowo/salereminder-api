using ProductApi.Models;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public interface IParserService
    {
        Task<Product> Parse(string productUrl);
    }
}
