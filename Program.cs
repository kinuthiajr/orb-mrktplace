using Orb.API;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Orb.API.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Orb.API.DTO;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:ValidIssuer"],
        ValidAudience = builder.Configuration["Jwt:ValidAudience"], 
        IssuerSigningKey = new SymmetricSecurityKey
        (Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]))
        
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine("Authentication failed: " + context.Exception.Message);
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token validated successfully");
            return Task.CompletedTask;
        }
    };
});


builder.Services.AddAuthorization(options => 
{


    options
    .AddPolicy("RequireSellerRole", policy => policy.RequireRole("Seller"));
    options
    .AddPolicy("RequireCustomerRole", policy => policy.RequireRole("Customer"));
    

    // options.AddPolicy("RequireSellerRole",policy =>
    // policy.RequireAssertion(context => 
    // context.User.HasClaim(hc => hc.Type == "UserType" && hc.Value == UserType.Seller.ToString()))
    // );

    // options.AddPolicy("RequireCustomerRole",
    // policy => policy.RequireAssertion(context => 
    // context.User.HasClaim(hc => hc.Type == "UserType" && hc.Value == UserType.Customer.ToString()))
    // );
});

builder.Services.AddControllers();

//Add IdentityUser and db store
builder.Services.AddIdentityCore<ApplicationUser>()
.AddEntityFrameworkStores<OrbDbContext>().AddApiEndpoints()
.AddDefaultTokenProviders();

// builder.Services.AddAuthentication()
//     .AddGoogle(options =>
//     {
//         options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
//         options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
//         options.CallbackPath = "/signin-google"; // Ensure this matches your Google Cloud Console configuration
//     });

//Confirmation email
builder.Services.Configure<IdentityOptions>(opt => {
    opt.SignIn.RequireConfirmedAccount = true;
});
builder.Services.AddTransient<IEmailSender, EmailService>();


builder.Services.AddDbContext<OrbDbContext>(options => 
options.UseNpgsql(builder.Configuration.GetConnectionString("DbConnectionString")));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt => {
        opt.Title = "OrbAPI";
    });
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGroup("api/account/").MapIdentityApi<ApplicationUser>();
var customerGroup = app.MapGroup("api/customers")
    .WithTags("Customer")
    .RequireAuthorization("RequireCustomerRole");
var sellerGroup = app.MapGroup("api/sellers")
    .WithTags("Seller")
    .RequireAuthorization("RequireSellerRole");


// sellerGroup.MapGet("shop", async (OrbDbContext db, ClaimsPrincipal user) => {
//     var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
//     return await db.Shops.FirstOrDefaultAsync(s => s.SellerId == sellerId);
// });

// sellerGroup.MapPost("products", async (OrbDbContext db, ClaimsPrincipal user, ProductDto productDto) => {
//     var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    
//     // Get the seller's shop
//     var shop = await db.Shops.FirstOrDefaultAsync(s => s.SellerId == sellerId);
    
//     if (shop == null)
//     {
//         return Results.BadRequest("You need to create a shop first before adding products.");
//     }
    
//     var product = new Product
//     {
//         Name = productDto.Name,
//         Description = productDto.Description,
//         Price = productDto.Price,
//         StockQuantity = productDto.StockQuantity,
//         ShopId = shop.Id
//     };
    
//     db.Products.Add(product);
//     await db.SaveChangesAsync();
    
//     return Results.Created($"/api/seller/products/{product.Id}", new {
//         Id = product.Id,
//         Name = product.Name,
//         Description = product.Description,
//         Price = product.Price,
//         StockQuantity = product.StockQuantity,
//         ShopId = shop.Id,
//         ShopName = shop.Name
//     });
// });

// // Add a GET endpoint to retrieve a specific product
// sellerGroup.MapGet("products/{id}", async (OrbDbContext db, ClaimsPrincipal user, Guid id) => {
//     var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    
//     var product = await db.Products
//         .Include(p => p.Shop)
//         .FirstOrDefaultAsync(p => p.Id == id && p.Shop.SellerId == sellerId);
        
//     if (product == null)
//     {
//         return Results.NotFound();
//     }
    
//     return Results.Ok(new {
//         Id = product.Id,
//         Name = product.Name,
//         Description = product.Description,
//         Price = product.Price,
//         StockQuantity = product.StockQuantity,
//         ShopId = product.ShopId,
//         ShopName = product.Shop.Name
//     });
// });

// // Add a GET endpoint to list all products for the seller
// sellerGroup.MapGet("products", async (OrbDbContext db, ClaimsPrincipal user) => {
//     var sellerId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    
//     var products = await db.Products
//         .Include(p => p.Shop)
//         .Where(p => p.Shop.SellerId == sellerId)
//         .Select(p => new {
//             Id = p.Id,
//             Name = p.Name,
//             Description = p.Description,
//             Price = p.Price,
//             StockQuantity = p.StockQuantity,
//             ShopId = p.ShopId,
//             ShopName = p.Shop.Name
//         })
//         .ToListAsync();
        
//     return Results.Ok(products);
// });


var commonGroup = app.MapGroup("api/common")
    .WithTags("Common");
app.Run();

