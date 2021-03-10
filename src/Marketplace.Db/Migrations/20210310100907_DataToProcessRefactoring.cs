using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class DataToProcessRefactoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NftIncomeTransactions_Deposited_LockTime",
                table: "NftIncomeTransactions");

            migrationBuilder.DropColumn(
                name: "Deposited",
                table: "NftIncomeTransactions");

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "NftIncomeTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "NftIncomeTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ErrorMessage",
                table: "KusamaTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockTime",
                table: "KusamaTransactions",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "KusamaTransactions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomeTransactions_Status_LockTime",
                table: "NftIncomeTransactions",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_KusamaTransactions_Status_LockTime",
                table: "KusamaTransactions",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NftIncomeTransactions_Status_LockTime",
                table: "NftIncomeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_KusamaTransactions_Status_LockTime",
                table: "KusamaTransactions");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "NftIncomeTransactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "NftIncomeTransactions");

            migrationBuilder.DropColumn(
                name: "ErrorMessage",
                table: "KusamaTransactions");

            migrationBuilder.DropColumn(
                name: "LockTime",
                table: "KusamaTransactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "KusamaTransactions");

            migrationBuilder.AddColumn<bool>(
                name: "Deposited",
                table: "NftIncomeTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomeTransactions_Deposited_LockTime",
                table: "NftIncomeTransactions",
                columns: new[] { "Deposited", "LockTime" },
                filter: "\"Deposited\" is not true");
        }
    }
}
