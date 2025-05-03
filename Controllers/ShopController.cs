using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orb.API.DTO;
using Orb.API.Models;

namespace Orb.API.Controllers
{
    [ApiController]
    [Route("api/shops")]
    [Tags("Shops")]
    public class ShopController : ControllerBase
    {
        private readonly OrbDbContext _context;

        public ShopController(OrbDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ShopDto>>> GetShops()
        {
            var shops = await _context.Shops
            .Select(shp => new ShopDto{
                Name = shp.Name,
                Description = shp.Description,
                Location = shp.Location,
                Slug = shp.Slug,
            }).ToListAsync();
            return shops;
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ShopDetailDto>> GetShopById(Guid id)
        {
            var shop = await _context.Shops
            .Include(shp => shp.Products)
            .FirstOrDefaultAsync(shp => shp.Id == id);
            
            if (shop == null)
            {
                return NotFound();
            }
            
            return CreateShopDetailDto(shop);
        }

            

            [HttpGet("{slug}")]
            public async Task<ActionResult<ShopDetailDto>> GetShopBySlug(string slug)
            {
                var shop = await _context.Shops
                .Include(shp => shp.Products)
                .FirstOrDefaultAsync(shp => shp.Slug ==  slug);

                if (shop == null)
                {
                    return NotFound();
                }

                return CreateShopDetailDto(shop);
            }
    
        
            //--------------Post------------//
            [HttpPost]
            [Authorize(Policy = "RequireSellerRole")]
            
            public async Task<ActionResult<ShopDto>> CreateShop(CreateShopDto shopDto)
            {
                var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Check if seller already has a shop
                var existingShop = await _context.Shops
                .FirstOrDefaultAsync(shp => shp.SellerId == sellerID);

                if (existingShop != null)
                {
                    return BadRequest("You already have a shop");
                }

                string slug = GenerateSlug(shopDto.Name); //Generate slug from name

                var slugExists = await _context.Shops.AnyAsync(s => s.Slug == slug);
                if (slugExists)
                {
                    slug = $"{slug}-{Guid.NewGuid().ToString().Substring(0, 8)}";
                }

                var shop = new Shop
                {
                    Name = shopDto.Name,
                    Description = shopDto.Description,
                    Location = shopDto.Location,
                    SellerId = sellerID,
                    Slug = slug
                };
                _context.Shops.Add(shop);
                await _context.SaveChangesAsync();

                //Update seller ShopId
                var seller = await _context.Users.FindAsync(sellerID);
                if (seller != null)
                {
                    seller.ShopId = shop.Id;
                    await _context.SaveChangesAsync();
                }

                return CreatedAtAction(
               nameof(GetShopBySlug),
                // This would be a named route you define
                new { slug = shop.Slug},
                new ShopDto
                {
                    Id = shop.Id,
                    Name = shop.Name,
                    Description = shop.Description,
                    Location = shop.Location,
                    Slug = shop.Slug
                });

            }

            [HttpGet("{slug}/products")]
            public async Task<ActionResult<IEnumerable<ProductDto>>> GetShopProducts(string slug)
            {
                var shop = await _context.Shops
                .FirstOrDefaultAsync(shp => shp.Slug == slug);

                if (shop == null)
                {
                    return NotFound("Shop not found");
                }

                var products = await _context.Products
                .Where(p => p.ShopId == shop.Id)
                .Select(p => new ProductDto
                {
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity

                }).ToListAsync();
                return Ok(products);
            }

            private ShopDetailDto CreateShopDetailDto(Shop shop)
            {
                return new ShopDetailDto
                {
                    Id = shop.Id,
                    Name = shop.Name,
                    Description = shop.Description,
                    Location = shop.Location,
                    Slug = shop.Slug,
                    Products = shop.Products.Select(p => new ProductDto
                    {
                        
                        Name = p.Name,
                        Description = p.Description,
                        Price = p.Price,
                        StockQuantity = p.StockQuantity
                    }).ToList()
                };
            }

            private string GenerateSlug(string name)
            {
                string slug = name.ToLowerInvariant();
                
                slug = RemoveDiacritics(slug); // Remove accents
                
                slug = slug.Replace(" ", "-");
                
                slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");
                
                slug = Regex.Replace(slug, @"-+", "-");
                
                slug = slug.Trim('-');
                
                return slug;
            }


            private string RemoveDiacritics(string text)
            {
                var normalizedString = text.Normalize(NormalizationForm.FormD);
                var stringBuilder = new StringBuilder();

                foreach (var c in normalizedString)
                {
                    var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                    {
                        stringBuilder.Append(c);
                    }
                }

                return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            }
            
        }

    }
