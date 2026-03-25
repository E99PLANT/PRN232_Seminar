using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AuthService.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "customer_auth");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "customer_auth",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    RegisterOtpHash = table.Column<string>(type: "text", nullable: true),
                    RegisterOtpExpiresAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RegisterOtpFailCount = table.Column<int>(type: "integer", nullable: false),
                    RegisterOtpVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventStoreRecords",
                schema: "customer_auth",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AggregateId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EventType = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    OccurredOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventStoreRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventStoreRecords_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "customer_auth",
                        principalTable: "Accounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Accounts_Email",
                schema: "customer_auth",
                table: "Accounts",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventStoreRecords_AccountId",
                schema: "customer_auth",
                table: "EventStoreRecords",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_EventStoreRecords_AggregateId",
                schema: "customer_auth",
                table: "EventStoreRecords",
                column: "AggregateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventStoreRecords",
                schema: "customer_auth");

            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "customer_auth");
        }
    }
}
