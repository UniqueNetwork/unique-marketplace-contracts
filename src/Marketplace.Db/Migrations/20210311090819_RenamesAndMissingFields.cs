using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class RenamesAndMissingFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KusamaIncomeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Offers_CollectionId",
                table: "Offers");

            migrationBuilder.AddColumn<decimal>(
                name: "QuoteId",
                table: "Offers",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 2m);

            migrationBuilder.CreateTable(
                name: "NftOutgoingTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TokenId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    RecipientPublicKey = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LockTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NftOutgoingTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuoteIncomeTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<string>(type: "text", nullable: false),
                    QuoteId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AccountPublicKey = table.Column<string>(type: "text", nullable: false),
                    BlockId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LockTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuoteIncomeTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_OfferStatus_CollectionId_TokenId",
                table: "Offers",
                columns: new[] { "OfferStatus", "CollectionId", "TokenId" });

            migrationBuilder.CreateIndex(
                name: "IX_NftOutgoingTransactions_Status_LockTime",
                table: "NftOutgoingTransactions",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteIncomeTransactions_AccountPublicKey",
                table: "QuoteIncomeTransactions",
                column: "AccountPublicKey");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteIncomeTransactions_Status_LockTime",
                table: "QuoteIncomeTransactions",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NftOutgoingTransactions");

            migrationBuilder.DropTable(
                name: "QuoteIncomeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Offers_OfferStatus_CollectionId_TokenId",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "QuoteId",
                table: "Offers");

            migrationBuilder.CreateTable(
                name: "KusamaIncomeTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountPublicKey = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<string>(type: "text", nullable: false),
                    BlockId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    LockTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    QuoteId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KusamaIncomeTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_CollectionId",
                table: "Offers",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_KusamaIncomeTransactions_AccountPublicKey",
                table: "KusamaIncomeTransactions",
                column: "AccountPublicKey");

            migrationBuilder.CreateIndex(
                name: "IX_KusamaIncomeTransactions_Status_LockTime",
                table: "KusamaIncomeTransactions",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");
        }
    }
}
