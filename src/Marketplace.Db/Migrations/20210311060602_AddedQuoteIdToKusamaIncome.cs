using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class AddedQuoteIdToKusamaIncome : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "QuoteId",
                table: "KusamaIncomeTransactions",
                type: "numeric(20,0)",
                nullable: false,
                defaultValue: 2m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QuoteId",
                table: "KusamaIncomeTransactions");
        }
    }
}
