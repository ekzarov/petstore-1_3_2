using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Petstore.Data.Migrations
{
    /// <inheritdoc />
    public partial class FullLegacyCatalogSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CatalogItems",
                columns: new[] { "Id", "Attributes", "Currency", "Description", "Name", "Price", "ProductId" },
                values: new object[,]
                {
                    { "EST-20", "[\"Adult Male\"]", "USD", "Fresh Water fish from China", "Adult Male Goldfish", 5.50m, "FI-FW-02" },
                    { "EST-21", "[\"Adult Female\"]", "USD", "Fresh Water fish from China", "Adult Female Goldfish", 5.29m, "FI-FW-02" }
                });

            migrationBuilder.InsertData(
                table: "CatalogProducts",
                columns: new[] { "Id", "CategoryId", "Description", "Name" },
                values: new object[,]
                {
                    { "AV-CB-01", "BIRDS", "Great companion for up to 75 years", "Amazon Parrot" },
                    { "AV-SB-02", "BIRDS", "Great stress reliever", "Finch" },
                    { "FI-FW-01", "FISH", "Fresh Water fish from Japan", "Koi" },
                    { "FI-SW-02", "FISH", "Salt Water fish from Australia", "Tiger Shark" },
                    { "FL-DLH-02", "CATS", "Friendly house cat, doubles as a princess", "Persian" },
                    { "FL-DSH-01", "CATS", "Great for reducing mouse populations", "Manx" },
                    { "K9-BD-01", "DOGS", "Friendly dog from England", "Bulldog" },
                    { "K9-CW-01", "DOGS", "Great companion dog", "Chihuahua" },
                    { "K9-DL-01", "DOGS", "Great dog for a Fire Station", "Dalmation" },
                    { "K9-PO-02", "DOGS", "Cute dog from France", "Poodle" },
                    { "K9-RT-01", "DOGS", "Great family dog", "Golden Retriever" },
                    { "K9-RT-02", "DOGS", "Great hunting dog", "Labrador Retriever" },
                    { "RP-LI-02", "REPTILES", "Friendly green friend", "Iguana" },
                    { "RP-SN-01", "REPTILES", "Doubles as a watch dog", "Rattlesnake" }
                });

            migrationBuilder.InsertData(
                table: "CatalogItems",
                columns: new[] { "Id", "Attributes", "Currency", "Description", "Name", "Price", "ProductId" },
                values: new object[,]
                {
                    { "EST-10", "[\"Spotted Adult Female\"]", "USD", "Great dog for a Fire Station", "Spotted Adult Female Dalmation", 18.50m, "K9-DL-01" },
                    { "EST-11", "[\"Venomless\"]", "USD", "More Bark than bite", "Venomless Rattlesnake", 18.50m, "RP-SN-01" },
                    { "EST-12", "[\"Rattleless\"]", "USD", "Doubles as a watch dog", "Rattleless Rattlesnake", 18.50m, "RP-SN-01" },
                    { "EST-13", "[\"Green Adult\"]", "USD", "Friendly green friend", "Green Adult Iguana", 12.50m, "RP-LI-02" },
                    { "EST-14", "[\"Tailless\"]", "USD", "Great for reducing mouse populations", "Tailless Manx", 58.50m, "FL-DSH-01" },
                    { "EST-15", "[\"With tail\"]", "USD", "Great for reducing mouse populations", "With tail Manx", 23.50m, "FL-DSH-01" },
                    { "EST-16", "[\"Adult Female\"]", "USD", "Friendly house cat, doubles as a princess", "Adult Female Persian", 93.50m, "FL-DLH-02" },
                    { "EST-17", "[\"Adult Male\"]", "USD", "Friendly house cat, doubles as a prince", "Adult Male Persian", 93.50m, "FL-DLH-02" },
                    { "EST-18", "[\"Adult Male\"]", "USD", "Great companion for up to 75 years", "Adult Male Amazon Parrot", 193.50m, "AV-CB-01" },
                    { "EST-19", "[\"Adult Male\"]", "USD", "Great stress reliever", "Adult Male Finch", 15.50m, "AV-SB-02" },
                    { "EST-22", "[\"Adult Male\"]", "USD", "Great hunting dog", "Adult Male Labrador Retriever", 135.50m, "K9-RT-02" },
                    { "EST-23", "[\"Adult Female\"]", "USD", "Great hunting dog", "Adult Female Labrador Retriever", 145.49m, "K9-RT-02" },
                    { "EST-24", "[\"Male Puppy\"]", "USD", "Great addition to a family", "Male Puppy Labrador Retriever", 255.50m, "K9-RT-02" },
                    { "EST-25", "[\"Female Puppy\"]", "USD", "Great hunting dog", "Female Puppy Labrador Retriever", 325.29m, "K9-RT-02" },
                    { "EST-26", "[\"Adult Male\"]", "USD", "Little yapper", "Adult Male Chihuahua", 125.50m, "K9-CW-01" },
                    { "EST-27", "[\"Adult Female\"]", "USD", "Great companion dog", "Adult Female Chihuahua", 155.29m, "K9-CW-01" },
                    { "EST-28", "[\"Adult Female\"]", "USD", "Great family dog", "Adult Female Golden Retriever", 155.29m, "K9-RT-01" },
                    { "EST-3", "[\"Toothless\",\"Mean\"]", "USD", "Salt Water fish from Australia", "Toothless Tiger Shark", 18.50m, "FI-SW-02" },
                    { "EST-4", "[\"Spotted\"]", "USD", "Fresh Water fish from Japan", "Spotted Koi", 18.50m, "FI-FW-01" },
                    { "EST-5", "[\"Spotless\"]", "USD", "Fresh Water fish from Japan", "Spotless Koi", 18.50m, "FI-FW-01" },
                    { "EST-6", "[\"Male Adult\"]", "USD", "Friendly dog from England", "Male Adult Bulldog", 18.50m, "K9-BD-01" },
                    { "EST-7", "[\"Female Puppy\"]", "USD", "Friendly dog from England", "Female Puppy Bulldog", 18.50m, "K9-BD-01" },
                    { "EST-8", "[\"Male Puppy\"]", "USD", "Cute dog from France", "Male Puppy Poodle", 18.50m, "K9-PO-02" },
                    { "EST-9", "[\"Spotless Male Puppy\"]", "USD", "Great dog for a Fire Station", "Spotless Male Puppy Dalmation", 18.50m, "K9-DL-01" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-10");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-11");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-12");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-13");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-14");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-15");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-16");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-17");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-18");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-19");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-20");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-21");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-22");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-23");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-24");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-25");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-26");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-27");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-28");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-3");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-4");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-5");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-6");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-7");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-8");

            migrationBuilder.DeleteData(
                table: "CatalogItems",
                keyColumn: "Id",
                keyValue: "EST-9");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "AV-CB-01");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "AV-SB-02");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "FI-FW-01");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "FI-SW-02");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "FL-DLH-02");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "FL-DSH-01");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "K9-BD-01");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "K9-CW-01");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "K9-DL-01");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "K9-PO-02");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "K9-RT-01");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "K9-RT-02");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "RP-LI-02");

            migrationBuilder.DeleteData(
                table: "CatalogProducts",
                keyColumn: "Id",
                keyValue: "RP-SN-01");
        }
    }
}
