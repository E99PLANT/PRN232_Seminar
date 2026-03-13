using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PaymentService.Migrations
{
    /// <inheritdoc />
    public partial class AddEventIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                schema: "khanh_wallet",
                table: "WalletEvents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PreviousHash",
                schema: "khanh_wallet",
                table: "WalletEvents",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            // === DB TRIGGER: Chặn UPDATE/DELETE trên WalletEvents ===
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION khanh_wallet.prevent_event_modification()
                RETURNS TRIGGER AS $$
                BEGIN
                    RAISE EXCEPTION 'Wallet events are IMMUTABLE — cannot UPDATE or DELETE. Event Sourcing requires all events to be preserved.';
                    RETURN NULL;
                END;
                $$ LANGUAGE plpgsql;

                CREATE TRIGGER trg_prevent_wallet_event_modification
                BEFORE UPDATE OR DELETE ON khanh_wallet.""WalletEvents""
                FOR EACH ROW
                EXECUTE FUNCTION khanh_wallet.prevent_event_modification();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP TRIGGER IF EXISTS trg_prevent_wallet_event_modification ON khanh_wallet.""WalletEvents"";
                DROP FUNCTION IF EXISTS khanh_wallet.prevent_event_modification();
            ");

            migrationBuilder.DropColumn(
                name: "Hash",
                schema: "khanh_wallet",
                table: "WalletEvents");

            migrationBuilder.DropColumn(
                name: "PreviousHash",
                schema: "khanh_wallet",
                table: "WalletEvents");
        }
    }
}
