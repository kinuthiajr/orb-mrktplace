using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orb.API.DTO;
using Orb.API.Models;

namespace Orb.API.Controllers
{
    [ApiController]
    [Route("api/shops/{shopId}/products")]
    [Tags("Seller")]
    [Authorize(Policy = "RequireSellerRole")]
    public class SellerProductController : ControllerBase
    {
        private readonly OrbDbContext _context;
        public SellerProductController(OrbDbContext context)
        {
            _context = context;
        }   

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetShopProducts(Guid shopId)
        {
            var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var shopProducts = await _context.Shops.FirstOrDefaultAsync(shp =>
                shp.Id == shopId && shp.SellerId == sellerID
            );

            if (shopProducts == null)
            {
                return NotFound("Shop not found or you don't have access to it");
            }

            var products = await _context.Products.Where(p => p.ShopId == shopId)
            .ToListAsync();

            return Ok(products);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid shopId, Guid id)
        {
            var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var shop = await _context.Shops.FirstOrDefaultAsync(shp => 
            shp.Id == shopId && shp.SellerId == sellerID);

            if (shop == null)
            {
                return NotFound("Shop not found or you don't have access to it");
            }

            var product = await _context.Products
            .Where(p => p.ShopId == shopId && p.Id == id)
            .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound("Product not found");
            }

            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Guid shopId, ProductDto productDto)
        {
            var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var shop = await _context.Shops.FirstOrDefaultAsync(shp => 
            shp.Id == shopId && shp.SellerId == sellerID); //Verify the shop belongs to the seller

            if (shop == null)
            {
                return NotFound("Shop not found or you don't have access to it");
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                ShopId = shopId
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), 
            new { shopId = shopId, id = product.Id }, product);


        }
    }
}