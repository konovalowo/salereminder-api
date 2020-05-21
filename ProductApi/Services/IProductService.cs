using ProductApi.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public interface IProductService
    {
        public Task<Product> GetProduct(int productId);
        public Task<List<Product>> GetUserProducts(int userId);
        public Task<Product> PutUserProduct(int userId, string productUrl);
        public Task<Product> RemoveUserProduct(int userId, int productId);
    }
}
