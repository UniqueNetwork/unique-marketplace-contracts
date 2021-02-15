using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class KusamaVault : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "KusamaProcessedBlocks",
                columns: table => new
                {
                    BlockNumber = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ProcessDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KusamaProcessedBlocks", x => x.BlockNumber);
                });

            migrationBuilder.CreateTable(
                name: "KusamaTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AccountPublicKey = table.Column<string>(type: "text", nullable: false),
                    BlockId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KusamaTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_KusamaTransactions_KusamaProcessedBlocks_BlockId",
                        column: x => x.BlockId,
                        principalTable: "KusamaProcessedBlocks",
                        principalColumn: "BlockNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KusamaTransactions_AccountPublicKey",
                table: "KusamaTransactions",
                column: "AccountPublicKey");

            migrationBuilder.CreateIndex(
                name: "IX_KusamaTransactions_BlockId",
                table: "KusamaTransactions",
                column: "BlockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KusamaTransactions");

            migrationBuilder.DropTable(
                name: "KusamaProcessedBlocks");
        }
    }
}
