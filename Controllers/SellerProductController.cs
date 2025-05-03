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
    [Route("api/seller/products")]
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
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetSellerProducts()
        {
            var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var shopProducts = await _context.Products
            .Include(shp => shp.Shop)
            .Where(p => p.Shop.SellerId == sellerID)
            .Select(p => new ProductDto
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                ShopId = p.ShopId,
                ShopName = p.Shop.Name,
                ProductSlug = p.Shop.ShopSlug
            }).ToListAsync();

            return Ok(shopProducts);
        }
        
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetSellerProduct(Guid id)
        {
            var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products
            .Include(shp => shp.Shop)
            .FirstOrDefaultAsync(p => p.Id == id
            && p.Shop.SellerId == sellerID);

            if (product == null)
            {
                return NotFound("Product not found or you don't have access to it");
            }

            return new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity, 
                ShopName = product.Shop.Name,
                ProductSlug = product.Shop.ShopSlug,
                ShopId = product.Shop.Id,
            };
        }

        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct(ProductCreateDto productDto)
        {
            
            var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var shop = await _context.Shops.FirstOrDefaultAsync(shp => 
            shp.SellerId == sellerID); //Get seller shop

            if (shop == null)
            {
                return BadRequest("You need to create a shop first before adding products.");
            }

            //generate slug from product name 
            string baseSlug = GenerateSlug(productDto.Name);
            string productSlug = baseSlug;

            //check slug already in the shop
            int counter = 1;
            while(await _context.Products.AnyAsync(p => p.ShopId == shop.Id 
            && p.ProductSlug == productSlug))
            {
                //append counter to slug
                productSlug = $"{baseSlug}-{counter}";
                counter++;
            }

            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                StockQuantity = productDto.StockQuantity,
                ProductSlug = productSlug,
                ShopId = shop.Id,
                
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var productResponse = new ProductDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                ShopName = shop.Name,
                ProductSlug = product.ProductSlug,
                ShopId = product.ShopId,
                FullSlug = $"{shop.ShopSlug}/{product.ProductSlug}"
            };
            

            return CreatedAtAction(nameof(GetSellerProduct), 
            new {id = product.Id }, productResponse);

        }

        private string GenerateSlug(string name)
        {
            string slug = name.ToLowerInvariant();

            slug = slug.Replace(" ", "-");
            
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
            
            slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
            
            slug = slug.Trim('-');
            
            return slug;
        }
    }
}