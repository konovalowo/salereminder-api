using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Services
{
    public class ProductsService : IProductService
    {
        private readonly ProductContext _context;

        private readonly IParserService _parserService;

        private readonly ILogger<ProductsService> _logger;

        public ProductsService(ProductContext context, ILogger<ProductsService> logger, IParserService parserService)
        {
            _context = context;
            _logger = logger;
            _parserService = parserService;
        }

        public async Task<Product> GetProduct(int productId)
        {
            return await _context.Products.FindAsync(productId);
        }

        public async Task<List<Product>> GetUserProducts(int userId)
        {
            UserProfile profile = await _context.UserProfiles.Include(p => p.UserProducts)
                                                             .ThenInclude(p => p.Product)
                                                             .SingleAsync(p => p.UserId == userId);

            var productList = profile.UserProducts.Select(products => products.Product).ToList();
            return productList;
        }

        public async Task<Product> PutUserProduct(int userId, string productUrl)
        {
            UserProfile profile = await _context.UserProfiles.SingleAsync(p => p.UserId == userId);

            productUrl = productUrl.RemoveQueryString();

            // Check if a product already in db
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Url == productUrl);

            if (product == null)
            {
                product = await _parserService.Parse(productUrl);
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
            }

            profile.UserProducts.Add(new UserProfileProduct { UserProfileId = profile.Id, ProductId = product.Id });

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Added product (id = {product.Id})");

            return product;
        }

        public async Task<Product> RemoveUserProduct(int userId, int productId)
        {
            UserProfile profile = await _context.UserProfiles.Include(p => p.UserProducts)
                                                             .ThenInclude(p => p.Product)
                                                             .SingleAsync(p => p.UserId == userId);

            var userProfileProduct = profile.UserProducts.Single(p => p.ProductId == productId);
            profile.UserProducts.Remove(userProfileProduct);

            var product = userProfileProduct.Product;
            var productUserProductsList = _context.Products.Include(p => p.UserProducts)
                                                           .Single(u => u.Id == productId).UserProducts;

            if (productUserProductsList.DefaultIfEmpty() == null)
            {
                _context.Products.Remove(product);
                _logger.LogInformation($"Removed product (id = {productId})");
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation($"Removed product (id = {productId}) from user's list (id = {userId})");

            return product;
        }
    }
}
