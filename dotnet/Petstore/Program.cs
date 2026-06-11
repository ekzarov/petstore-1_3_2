using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Petstore.Accounts;
using Petstore.Catalog;
using Petstore.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<PetstoreCatalogContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PetstoreCatalog")
        ?? throw new InvalidOperationException("Connection string 'PetstoreCatalog' is not configured.")));
builder.Services.AddScoped<ICatalogRepository, CatalogRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<Petstore.Cart.ICartRepository, Petstore.Cart.CartRepository>();
builder.Services.AddScoped<Petstore.Cart.CartViewBuilder>();
builder.Services.AddScoped<Petstore.Orders.IOrderRepository, Petstore.Orders.OrderRepository>();
builder.Services.AddScoped<Petstore.Orders.OrderPlacementService>();
builder.Services.AddScoped<Petstore.OrderProcessing.OrderTransitionRepository>();
builder.Services.AddScoped<Petstore.OrderProcessing.IOrderProcessingService, Petstore.OrderProcessing.OrderProcessingService>();
builder.Services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
builder.Services.AddSingleton<JwtTokenService>();

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Configuration 'Jwt:Key' is not set.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Configuration 'Jwt:Issuer' is not set.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtIssuer,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<PetstoreCatalogContext>();
    var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
    if (await context.Database.CanConnectAsync())
    {
        await AccountSeeder.SeedAsync(context, passwordHasher);
    }
}

app.Run();

public partial class Program
{
}
