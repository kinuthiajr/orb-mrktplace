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
                Location = shp.Location
            }).ToListAsync();
            return shops;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ShopDetailDto>> GetShop(Guid id)
        {
            var shop = await _context.Shops
            .Include(shp => shp.Products)
            .FirstOrDefaultAsync(shp => shp.Id == id);
            
            if (shop == null)
            {
                return NotFound();
            }
            var shopDetail = new ShopDetailDto
            {
                
                Name = shop.Name,
                Description = shop.Description,
                Location = shop.Location,
                Products = shop.Products.Select(p => new ProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    StockQuantity = p.StockQuantity
                }).ToList()
            };

            return shopDetail;
        }
            //--------------Post------------//
            [HttpPost]
            [Authorize(Policy = "RequireSellerRole")]
            
            public async Task<ActionResult<ShopDto>> CreateShop(CreateShopDto shopDto)
            {
                var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var shop = new Shop
                {
                    Name = shopDto.Name,
                    Description = shopDto.Description,
                    Location = shopDto.Location,
                    SellerId = sellerID 
                };
                _context.Shops.Add(shop);
                await _context.SaveChangesAsync();

                return CreatedAtAction(
               nameof(GetShop),
                // This would be a named route you define
                new { id = shop.Id },
                new ShopDto
                {
                    
                    Name = shopDto.Name,
                    Description = shopDto.Description,
                    Location = shopDto.Location,
                    
                });

            }
        }

    }
