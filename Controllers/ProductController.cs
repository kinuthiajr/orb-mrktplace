using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orb.API.DTO;

namespace Orb.API.Controllers
{
    [ApiController]
    [Route("api/products")]
    [Tags("Products")]
    public class ProductController : ControllerBase
    {
        private readonly OrbDbContext _context;
        public ProductController(OrbDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            var products = await _context.Products
            .Include(shp => shp.Shop)
            .Select(p => new ProductDto
            {
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                // ShopName = p.Shop.Name
            }).ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
        {
            var product = await _context.Products
            .Include(p => p.Shop)
            .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var productDto = new ProductDto
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity
            };

            return Ok(product);
        }

        [HttpGet("shop/{shopId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByShop(Guid shopId)
        {
            var shop = await _context.Shops.FindAsync(shopId);
            if (shop == null)
            {
                return NotFound("No shop found ");
            }

            var products = await _context.Products
            .Where(p => p.ShopId == shopId)
            .Select(p => new ProductDto
            {
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity
            }).ToListAsync();

            return Ok(products);
        }
        




    }
}