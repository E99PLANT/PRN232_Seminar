using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace UserProfileService.Migrations
{
    /// <inheritdoc />
    public partial class InitUserProfileService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customer_profile");

            migrationBuilder.CreateTable(
                name: "EventStoreRecords",
                schema: "customer_profile",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AggregateId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStoreRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserProfiles",
                schema: "customer_profile",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AccountId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Dob = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Gender = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProfiles", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventStoreRecords_AggregateId",
                schema: "customer_profile",
                table: "EventStoreRecords",
                column: "AggregateId");

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_AccountId",
                schema: "customer_profile",
                table: "UserProfiles",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProfiles_Email",
                schema: "customer_profile",
                table: "UserProfiles",
                column: "Email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventStoreRecords",
                schema: "customer_profile");

            migrationBuilder.DropTable(
                name: "UserProfiles",
                schema: "customer_profile");
        }
    }
}
