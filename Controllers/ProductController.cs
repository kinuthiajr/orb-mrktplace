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
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ShopName = p.Shop.Name,
                ProductSlug = p.Shop.ShopSlug,
                FullSlug = p.Shop.ShopSlug + "/" + p.ProductSlug
            }).ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ProductDto>> GetProductById(Guid id)
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
                StockQuantity = product.StockQuantity,
                ShopName = product.Shop.Name,
                ProductSlug = product.ProductSlug
            };

            return Ok(product);
        }

        [HttpGet("shop/{shopId}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> 
        GetProductsByShop(Guid shopId)
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

        [HttpGet("{slug}")]
        public async Task<ActionResult<ProductDto>> GetProductBySlug(string slug)
        {
            if (Guid.TryParse(slug, out var id))
            {
                return await GetProductById(id);
            }

            var product = await _context.Products
            .Include(shp => shp.Shop)
            .FirstOrDefaultAsync(p => p.ProductSlug == slug);

            if (product == null)
            {
                return NotFound();
            }

            var productDto = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ShopName = product.Shop.Name,
                ProductSlug = product.ProductSlug
            };

            return Ok(productDto);
        }   


        [HttpGet("shop/{shopId:guid}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByShopId(Guid shopId)
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
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ShopId = p.ShopId,
                ShopName = shop.Name,
                ProductSlug = p.ProductSlug 
            }).ToListAsync();

            return Ok(products);
        }

        // Combined slug for shop and product
        [HttpGet("by-path/{*fullpath}")]
        public async Task<ActionResult<ProductDto>> GetProductByFullPath(string fullPath)
        {
            var pathParts = fullPath.Split("/");

            if (pathParts.Length != 2)
            {
                return BadRequest
                ("Invalid path format. Expected format: {shopSlug}/{productSlug}");
            }

            var shopSlug = pathParts[0];
            var productSlug = pathParts[1];

            var product = await _context.Products
            .Include(shp => shp.Shop)
            .FirstOrDefaultAsync(p => p.Shop.ShopSlug == shopSlug && p.ProductSlug == productSlug);

             if (product == null)
            {
                return NotFound("Product not found");
            }

            return Ok(new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ShopName = product.Shop.Name,
                ProductSlug = product.ProductSlug,
                FullSlug = $"{product.Shop.ShopSlug}/{product.ProductSlug}"
            });

            
        }

    }
}