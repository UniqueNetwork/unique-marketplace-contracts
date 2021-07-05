using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class OnHold : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OfferId",
                table: "NftIncomingTransaction",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomingTransaction_OfferId",
                table: "NftIncomingTransaction",
                column: "OfferId");

            migrationBuilder.AddForeignKey(
                name: "FK_NftIncomingTransaction_Offer_OfferId",
                table: "NftIncomingTransaction",
                column: "OfferId",
                principalTable: "Offer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NftIncomingTransaction_Offer_OfferId",
                table: "NftIncomingTransaction");

            migrationBuilder.DropIndex(
                name: "IX_NftIncomingTransaction_OfferId",
                table: "NftIncomingTransaction");

            migrationBuilder.DropColumn(
                name: "OfferId",
                table: "NftIncomingTransaction");
        }
    }
}
