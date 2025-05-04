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
    [Route("api/seller/shop")]
    public class SellerShopController : ControllerBase
    {
        private readonly OrbDbContext _context;

        public SellerShopController(OrbDbContext context)
        {
            _context = context;
        }   

        [HttpGet("seller/me")]
        [Authorize(Policy = "RequireSellerRole")]
        public async Task<ActionResult<SellerProfileDto>> GetMySellerProfile()
        {
            var sellerID = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var seller = await _context.Users
            .Include(u => u.Shop)
            .FirstOrDefaultAsync(u => u.Id == sellerID && u.UserType == UserType.Seller);

            if (seller == null)
                return NotFound("Seller profile not found");

            var shop = seller.Shop;

            // product count
            int totalProducts = 0;

            if (shop != null)
            {
                totalProducts = await _context
                .Products.CountAsync(p => p.ShopId == shop.Id);
            }

            return new SellerProfileDto
            {
                Id = seller.Id,
                Email = seller.Email,
                ShopName = seller.ShopName,
                // ShopDescription = seller.Description,
                // JoinedDate = seller.CreatedAt,
                TotalProducts = totalProducts,
                Shop = shop == null ? null :new ShopProfileDto
                {
                    Id = shop.Id,
                    Name = shop.Name,
                    Location = shop.Location,
                    ShopSlug = shop.ShopSlug,
                    CreatedAt = shop.CreatedAt
                }
            };

        }


    }
}