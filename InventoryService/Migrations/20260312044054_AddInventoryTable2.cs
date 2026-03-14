using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InventoryService.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryTable2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventories",
                schema: "danh_inventory",
                table: "Inventories");

            migrationBuilder.RenameTable(
                name: "Inventories",
                schema: "danh_inventory",
                newName: "Inventory",
                newSchema: "danh_inventory");

            migrationBuilder.RenameIndex(
                name: "IX_Inventories_ProductId",
                schema: "danh_inventory",
                table: "Inventory",
                newName: "IX_Inventory_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventory",
                schema: "danh_inventory",
                table: "Inventory",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Inventory",
                schema: "danh_inventory",
                table: "Inventory");

            migrationBuilder.RenameTable(
                name: "Inventory",
                schema: "danh_inventory",
                newName: "Inventories",
                newSchema: "danh_inventory");

            migrationBuilder.RenameIndex(
                name: "IX_Inventory_ProductId",
                schema: "danh_inventory",
                table: "Inventories",
                newName: "IX_Inventories_ProductId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Inventories",
                schema: "danh_inventory",
                table: "Inventories",
                column: "Id");
        }
    }
}
