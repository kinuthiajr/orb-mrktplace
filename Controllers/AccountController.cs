using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Orb.API.DTO;
using Orb.API.Models;

namespace Orb.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Tags("Common")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly OrbDbContext _context;
        
        public AccountController(UserManager<ApplicationUser> userManager, 
        IEmailSender emailSender, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, OrbDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register-customer")]
        [Tags("Customer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CustomerRegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                UserType = UserType.Customer,
                ShippingAddress = model.ShippingAddress
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim("UserType", UserType.Customer.ToString()));
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account", 
                    new { userId = user.Id, token = token }, Request.Scheme);
                
                // Send confirmation email
                await _emailSender.SendEmailAsync(model.Email, "Confirm your email",
                    $"Please confirm your account by clicking this link: {confirmationLink}");
                
                return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
            }
            return BadRequest(result.Errors);
        }

        [HttpPost("register-seller")]
        [Tags("Seller")]
        public async Task<IActionResult> RegisterSeller([FromBody] SellerRegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                UserType = UserType.Seller,
                ShopName = model.ShopName,
                ShopDescription = model.ShopDescription
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddClaimAsync(user, new Claim("UserType", UserType.Seller.ToString()));
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action("ConfirmEmail", "Account",new { userId = user.Id, token = token }, Request.Scheme);
                // Send confirmation email
                await _emailSender.SendEmailAsync(model.Email, "Confirm your email",$"Please confirm your account by clicking this link: {confirmationLink}");
                return Ok(new { message = "Registration successful. Please check your email to confirm your account." });
            }
            return BadRequest(result.Errors);
        }

        [HttpGet("confirm-email")]
        [Tags("Common")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return BadRequest("User ID and token are required");
                
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound($"Unable to load user with ID '{userId}'.");
                
            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded)
                return Ok("Thank you for confirming your email.");
                
            return BadRequest("Error confirming your email.");
        }


        //------------------Login section-----------------//

        [HttpPost("customer/login")]
        [Tags("Customer")]
        public async Task<IActionResult> CustomerLogin([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
                return BadRequest("Invalid email or password");
            
            if (user.UserType != UserType.Customer)
                return BadRequest("Invalid user type");
            
            var token = await GenerateJwtToken(user);
            
            var response = new CustomerLoginResponse
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                ShippingAddress = user.ShippingAddress,
                Expiration = DateTime.UtcNow.AddDays(4)
            };

            return Ok(response);
        }


        [HttpPost("seller/login")]
        [Tags("Seller")]
        public async Task<IActionResult> SellerLogin([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
            return BadRequest("Invalid email or password");

            if (user.UserType != UserType.Seller)
            return BadRequest("Invalid user type");

            var token = await GenerateJwtToken(user);
            var response = new SellerLoginResponse
            {
                Token = token,
                UserId = user.Id,
                Email = user.Email,
                ShopName = user.ShopName,
                ShopDescription = user.ShopDescription,
                Expiration = DateTime.UtcNow.AddDays(4) 
            };
            return Ok(response);
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            // var userClaims = await _userManager.GetClaimsAsync(user);

            Console.WriteLine($"Token generation - ValidIssuer: {_configuration["Jwt:ValidIssuer"]}");
    Console.WriteLine($"Token generation - ValidAudience: {_configuration["Jwt:ValidAudience"]}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UserType", user.UserType.ToString()),
                new Claim(ClaimTypes.Role, user.UserType.ToString())
            };
            // claims.AddRange(userClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:ValidIssuer"],
                audience: _configuration["Jwt:ValidAudience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(4),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }






        //---------------Product section ---------------//

    //      [HttpGet("{id}")]
    //      [Authorize(Policy = "RequireSellerRole")]
    //     [Tags("Seller")]
    //     public async Task<ActionResult<Product>> GetProduct(Guid id)
    //     {
    //         var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    //         var product = await _context.Products
    //             .Where(p => p.Shop.SellerId == sellerId && p.Id == id)
    //             .FirstOrDefaultAsync();

    //         if (product == null)
    //         {
    //             return NotFound();
    //         }

    //         return product;
    //     }


    //     [HttpGet]
    //     [Authorize(Policy = "RequireSellerRole")]
    //     [Tags("Seller")]
    //     public async Task<ActionResult<IEnumerable<Product>>> GetSellerProducts(Guid id)
    //     {
    //         var sellerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
    //         //check if the product belongs to the seller's shop
    //         var products = await _context.Products
    //             .Include(p => p.Shop)
    //             .Where(p => p.Shop.SellerId == sellerId && p.Id == id)
    //             .FirstOrDefaultAsync();
                
    //         return Ok(products);
    //     }
    // }

    public class CustomerRegisterModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ShippingAddress { get; set; }
    }

    public class SellerRegisterModel
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? ShopName { get; set; }
        public string? ShopDescription { get; set; }
    }

    public class LoginModel
    {
        public required string Email {get; set;}
        public required string Password {get; set;}
    }

    public class CustomerLoginResponse
        {
            public string Token { get; set; }
            public string UserId { get; set; }
            public string Email { get; set; }
            public string ShippingAddress { get; set; }
            public DateTime Expiration { get; set; }
        }
        
        // Seller login response
        public class SellerLoginResponse
        {
            public string Token { get; set; }
            public string UserId { get; set; }
            public string Email { get; set; }
            public string ShopName { get; set; }
            public string ShopDescription { get; set; }
            public DateTime Expiration { get; set; }
        }
    } 

}
