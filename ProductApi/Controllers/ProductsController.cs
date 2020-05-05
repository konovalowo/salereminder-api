using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductApi.Models;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using ProductApi.Services;

namespace ProductApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductContext _context;

        private readonly IParserService _parserService;

        private readonly ILogger _logger;

        public ProductsController(ProductContext context, ILogger<ProductsController> logger, IParserService parserService) 
        {
            _context = context;
            _logger = logger;
            _parserService = parserService;
        }

        // GET: api/Products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            _logger.LogInformation($"Get request for products");
            string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
            try
            {
                UserProfile profile = await _context.UserProfiles.Include(p => p.UserProducts).ThenInclude(p => p.Product)
                                                                 .SingleAsync(p => p.UserId.ToString() == currentUserId);
                var productList = profile.UserProducts.Select(products => products.Product).ToList();
                return productList;
            }
            catch (InvalidOperationException e)
            {
                _logger.LogInformation($"Get request failed: {e.Message}");
                return BadRequest();
            }
        }

        // GET: api/Products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }
            _logger.LogInformation($"Get request for product (id = {id})");
            return product;
        }

        // PUT: api/Products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            _context.Entry(product).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Products
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
            {
                string currentUserId = User.FindFirst(ClaimTypes.NameIdentifier).Value;
                UserProfile profile = await _context.UserProfiles.SingleAsync(p => p.UserId.ToString() == currentUserId);

                string productUrl = product.Url.RemoveQueryString();
                product = await _context.Products.FirstOrDefaultAsync(p => p.Url == productUrl);

                if (product == null)
                {
                    product = await _parserService.Parse(productUrl);
                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();
                }

                profile.UserProducts.Add(new UserProfileProduct { UserProfileId = profile.Id, ProductId = product.Id });

                await _context.SaveChangesAsync(); 

                _logger.LogInformation($"Post request: added product (id={product.Id})");
            }
            catch (NullReferenceException e)
            {
                _logger.LogInformation($"Post request failed: No such user, {e.Message}");
                return BadRequest();
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Post request failed: {e.Message}");
                return StatusCode(500);
            }

            return CreatedAtAction("GetProduct", new { id = product.Id }, product);
        }

        // DELETE: api/Products/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Product>> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Delete request: removed product (id={id})");
            return product;
        }

        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }
    }
}
