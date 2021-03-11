using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class RenamesKusamaToQuote : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_KusamaOutgoingTransactions",
                table: "KusamaOutgoingTransactions");

            migrationBuilder.RenameTable(
                name: "KusamaOutgoingTransactions",
                newName: "QuoteOutgoingTransactions");
    
            migrationBuilder.RenameIndex(
                name: "IX_KusamaOutgoingTransactions_Status",
                table: "QuoteOutgoingTransactions",
                newName: "IX_QuoteOutgoingTransactions_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteOutgoingTransactions",
                table: "QuoteOutgoingTransactions",
                column: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_QuoteOutgoingTransactions",
                table: "QuoteOutgoingTransactions");

            migrationBuilder.RenameTable(
                name: "QuoteOutgoingTransactions",
                newName: "KusamaOutgoingTransactions");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteOutgoingTransactions_Status",
                table: "KusamaOutgoingTransactions",
                newName: "IX_KusamaOutgoingTransactions_Status");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KusamaOutgoingTransactions",
                table: "KusamaOutgoingTransactions",
                column: "Id");
        }
    }
}
