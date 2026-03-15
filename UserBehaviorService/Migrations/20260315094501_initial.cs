using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserBehaviorService.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customer_behavior");

            migrationBuilder.CreateTable(
                name: "ConsumedMessages",
                schema: "customer_behavior",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ProcessedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumedMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserBehaviorProjections",
                schema: "customer_behavior",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    CurrentStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LoginCount = table.Column<int>(type: "integer", nullable: false),
                    FirstLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ProfileUpdateCount = table.Column<int>(type: "integer", nullable: false),
                    LastProfileUpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PreferredLoginHour = table.Column<int>(type: "integer", nullable: false),
                    MostActiveWeekday = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    AverageDaysBetweenLogins = table.Column<double>(type: "double precision", nullable: true),
                    EstimatedActiveDaysSpan = table.Column<int>(type: "integer", nullable: false),
                    AverageSessionDurationMinutes = table.Column<double>(type: "double precision", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBehaviorProjections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserLoginHistories",
                schema: "customer_behavior",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    LoggedInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LoginHour = table.Column<int>(type: "integer", nullable: false),
                    Weekday = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateOnlyUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLoginHistories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumedMessages_MessageId",
                schema: "customer_behavior",
                table: "ConsumedMessages",
                column: "MessageId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserBehaviorProjections_UserId",
                schema: "customer_behavior",
                table: "UserBehaviorProjections",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLoginHistories_UserId",
                schema: "customer_behavior",
                table: "UserLoginHistories",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumedMessages",
                schema: "customer_behavior");

            migrationBuilder.DropTable(
                name: "UserBehaviorProjections",
                schema: "customer_behavior");

            migrationBuilder.DropTable(
                name: "UserLoginHistories",
                schema: "customer_behavior");
        }
    }
}
