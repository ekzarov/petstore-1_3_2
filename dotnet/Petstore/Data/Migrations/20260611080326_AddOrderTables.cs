using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Petstore.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ShippingContact_FamilyName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShippingContact_GivenName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShippingContact_Street1 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ShippingContact_Street2 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    ShippingContact_City = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ShippingContact_State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ShippingContact_Zip = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    ShippingContact_Country = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ShippingContact_Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    ShippingContact_Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BillingContact_FamilyName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BillingContact_GivenName = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BillingContact_Street1 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    BillingContact_Street2 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true),
                    BillingContact_City = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BillingContact_State = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BillingContact_Zip = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    BillingContact_Country = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    BillingContact_Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    BillingContact_Phone = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderLines",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLines", x => new { x.OrderId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_OrderLines_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_Status",
                table: "Orders",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderLines");

            migrationBuilder.DropTable(
                name: "Orders");
        }
    }
}
