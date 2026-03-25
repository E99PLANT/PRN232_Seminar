using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UserBehaviorService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserLoginHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SessionDurationMinutes",
                schema: "customer_behavior",
                table: "UserLoginHistories",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionDurationMinutes",
                schema: "customer_behavior",
                table: "UserLoginHistories");
        }
    }
}
