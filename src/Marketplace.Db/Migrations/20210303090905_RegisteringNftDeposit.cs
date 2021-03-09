using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class RegisteringNftDeposit : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NftIncomeTransactions_Deposited",
                table: "NftIncomeTransactions");

            migrationBuilder.AddColumn<byte[]>(
                name: "SellerPublicKeyBytes",
                table: "Offers",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockTime",
                table: "NftIncomeTransactions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomeTransactions_Deposited_LockTime",
                table: "NftIncomeTransactions",
                columns: new[] { "Deposited", "LockTime" },
                filter: "\"Deposited\" is not true");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NftIncomeTransactions_Deposited_LockTime",
                table: "NftIncomeTransactions");

            migrationBuilder.DropColumn(
                name: "SellerPublicKeyBytes",
                table: "Offers");

            migrationBuilder.DropColumn(
                name: "LockTime",
                table: "NftIncomeTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomeTransactions_Deposited",
                table: "NftIncomeTransactions",
                column: "Deposited",
                filter: "\"Deposited\" is not true");
        }
    }
}
