using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    CollectionId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    TokenId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Price = table.Column<string>(type: "text", nullable: false),
                    Seller = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "text", nullable: false),
                    OfferStatus = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Trades",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TradeDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Buyer = table.Column<string>(type: "text", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trades_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Offers_CollectionId",
                table: "Offers",
                column: "CollectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_CreationDate",
                table: "Offers",
                column: "CreationDate");

            migrationBuilder.CreateIndex(
                name: "IX_Trades_OfferId",
                table: "Trades",
                column: "OfferId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Trades");

            migrationBuilder.DropTable(
                name: "Offers");
        }
    }
}
