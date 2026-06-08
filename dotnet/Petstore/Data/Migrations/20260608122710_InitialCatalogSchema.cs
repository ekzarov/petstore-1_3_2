using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Petstore.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCatalogSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CatalogCategories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CatalogProducts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogProducts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogProducts_CatalogCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "CatalogCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CatalogItems",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ProductId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Attributes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CatalogItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CatalogItems_CatalogProducts_ProductId",
                        column: x => x.ProductId,
                        principalTable: "CatalogProducts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CatalogItems_ProductId",
                table: "CatalogItems",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogProducts_CategoryId",
                table: "CatalogProducts",
                column: "CategoryId");

            migrationBuilder.InsertData(
                table: "CatalogCategories",
                columns: new[] { "Id", "Description", "Name" },
                values: new object[,]
                {
                    { "BIRDS", null, "Birds" },
                    { "CATS", null, "Cats" },
                    { "DOGS", null, "Dogs" },
                    { "FISH", null, "Fish" },
                    { "REPTILES", null, "Reptiles" }
                });

            migrationBuilder.InsertData(
                table: "CatalogProducts",
                columns: new[] { "Id", "CategoryId", "Description", "Name" },
                values: new object[,]
                {
                    { "FI-FW-02", "FISH", "Fresh Water fish from China", "Goldfish" },
                    { "FI-SW-01", "FISH", "Salt Water fish from Australia", "Angelfish" }
                });

            migrationBuilder.InsertData(
                table: "CatalogItems",
                columns: new[] { "Id", "Attributes", "Currency", "Description", "Name", "Price", "ProductId" },
                values: new object[,]
                {
                    { "EST-1", "[\"Large\",\"Cuddly\"]", "USD", "Fresh Water fish from Japan", "Large Angelfish", 16.50m, "FI-SW-01" },
                    { "EST-2", "[\"Small\"]", "USD", "Fresh Water fish from Japan", "Small Angelfish", 16.50m, "FI-SW-01" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CatalogItems");

            migrationBuilder.DropTable(
                name: "CatalogProducts");

            migrationBuilder.DropTable(
                name: "CatalogCategories");
        }
    }
}
