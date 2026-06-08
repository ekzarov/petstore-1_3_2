using Microsoft.EntityFrameworkCore;
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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}
