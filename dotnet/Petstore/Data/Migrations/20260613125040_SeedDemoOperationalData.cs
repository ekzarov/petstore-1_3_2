using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Petstore.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoOperationalData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = N'j2ee')
                BEGIN
                    INSERT INTO [Users] ([UserId], [PasswordHash], [PasswordSalt], [Role], [CreatedAt])
                    VALUES (N'j2ee', 0x5C706C3F6556DD9D2237104A6B27EB88FB796A6CE15A2275E58D644CA9046BA7, 0xC1537E8C3D0D597B4DA7EE9A1FEA9A8B, N'customer', '2026-01-01T00:00:00.0000000Z');
                END

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = N'j2ee-ja')
                BEGIN
                    INSERT INTO [Users] ([UserId], [PasswordHash], [PasswordSalt], [Role], [CreatedAt])
                    VALUES (N'j2ee-ja', 0x7C6D8DB7CE6301AD1423398F5DA2B5350AF875992C9E0C0C0717E2BAD64FC41F, 0xBED036D4804B8F53808A1F5C06293AAB, N'customer', '2026-01-01T00:00:00.0000000Z');
                END

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = N'shopper')
                BEGIN
                    INSERT INTO [Users] ([UserId], [PasswordHash], [PasswordSalt], [Role], [CreatedAt])
                    VALUES (N'shopper', 0xA594109DD7841C776052C5285CCB447B8CA71D1D13ECB6FC47B79B72D8C74238, 0x4688FEF3848FE017AA7CF62468C2173E, N'customer', '2026-01-01T00:00:00.0000000Z');
                END

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = N'admin')
                BEGIN
                    INSERT INTO [Users] ([UserId], [PasswordHash], [PasswordSalt], [Role], [CreatedAt])
                    VALUES (N'admin', 0x2A6CD82355DDA3A2211EE416FE8D320839166B4226E9E0D0233ECEA528BA06F0, 0xCA62B8E462C13682157E9FB2B9355C6D, N'admin', '2026-01-01T00:00:00.0000000Z');
                END

                IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserId] = N'supplier')
                BEGIN
                    INSERT INTO [Users] ([UserId], [PasswordHash], [PasswordSalt], [Role], [CreatedAt])
                    VALUES (N'supplier', 0xDBC13147C7367324F976D4FAE9E018E13ADCFE4655DC87CBDC77FB68377FCE8E, 0xBD02BDDAE31D1279711A7A8DC2502583, N'supplier', '2026-01-01T00:00:00.0000000Z');
                END
                """);

            migrationBuilder.Sql("""
                IF NOT EXISTS (SELECT 1 FROM [CustomerContacts] WHERE [UserId] = N'j2ee')
                BEGIN
                    INSERT INTO [CustomerContacts] ([UserId], [FamilyName], [GivenName], [Street1], [Street2], [City], [State], [Zip], [Country], [Email], [Phone])
                    VALUES (N'j2ee', N'Doe', N'J2EE', N'1 BluePrints Way', N'Suite 132', N'Palo Alto', N'CA', N'94303', N'USA', N'j2ee@petstore.example', N'555-0101');
                END

                IF NOT EXISTS (SELECT 1 FROM [CustomerContacts] WHERE [UserId] = N'admin')
                BEGIN
                    INSERT INTO [CustomerContacts] ([UserId], [FamilyName], [GivenName], [Street1], [Street2], [City], [State], [Zip], [Country], [Email], [Phone])
                    VALUES (N'admin', N'Admin', N'PetStore', N'500 Approval Plaza', N'Operations Desk', N'San Jose', N'CA', N'95113', N'USA', N'admin@petstore.example', N'555-0102');
                END

                IF NOT EXISTS (SELECT 1 FROM [CustomerContacts] WHERE [UserId] = N'supplier')
                BEGIN
                    INSERT INTO [CustomerContacts] ([UserId], [FamilyName], [GivenName], [Street1], [Street2], [City], [State], [Zip], [Country], [Email], [Phone])
                    VALUES (N'supplier', N'Supplier', N'PetStore', N'42 Inventory Road', N'Receiving Dock', N'Oakland', N'CA', N'94607', N'USA', N'supplier@petstore.example', N'555-0103');
                END
                """);

            migrationBuilder.Sql("""
                INSERT INTO [SupplierInventory] ([ItemId], [QuantityOnHand])
                SELECT [CatalogItems].[Id], 100
                FROM [CatalogItems]
                WHERE NOT EXISTS (
                    SELECT 1
                    FROM [SupplierInventory]
                    WHERE [SupplierInventory].[ItemId] = [CatalogItems].[Id]);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DELETE FROM [SupplierInventory]
                WHERE [QuantityOnHand] = 100
                  AND [ItemId] IN (SELECT [Id] FROM [CatalogItems]);

                DELETE FROM [CustomerContacts]
                WHERE [UserId] IN (N'j2ee', N'admin', N'supplier')
                  AND [Email] IN (N'j2ee@petstore.example', N'admin@petstore.example', N'supplier@petstore.example');

                DELETE FROM [Users]
                WHERE [UserId] IN (N'j2ee', N'j2ee-ja', N'shopper', N'admin', N'supplier')
                  AND [CreatedAt] = '2026-01-01T00:00:00.0000000Z';
                """);
        }
    }
}
