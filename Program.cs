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

Console.WriteLine($"JWT ValidIssuer: {builder.Configuration["Jwt:ValidIssuer"]}");
Console.WriteLine($"JWT ValidAudience: {builder.Configuration["Jwt:ValidAudience"]}");
Console.WriteLine($"JWT Secret (first 5 chars): {builder.Configuration["Jwt:Secret"]?.Substring(0, 5)}...");


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
var commonGroup = app.MapGroup("api/common")
    .WithTags("Common");
app.Run();

