using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class UniqueScanner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UniqueProcessedBlocks",
                columns: table => new
                {
                    BlockNumber = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    ProcessDate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UniqueProcessedBlocks", x => x.BlockNumber);
                });

            migrationBuilder.CreateTable(
                name: "NftIncomeTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<long>(type: "bigint", nullable: false),
                    TokenId = table.Column<long>(type: "bigint", nullable: false),
                    Deposited = table.Column<bool>(type: "boolean", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    OwnerPublicKey = table.Column<string>(type: "text", nullable: false),
                    UniqueProcessedBlockId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NftIncomeTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NftIncomeTransactions_UniqueProcessedBlocks_UniqueProcessed~",
                        column: x => x.UniqueProcessedBlockId,
                        principalTable: "UniqueProcessedBlocks",
                        principalColumn: "BlockNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomeTransactions_Deposited",
                table: "NftIncomeTransactions",
                column: "Deposited",
                filter: "\"Deposited\" is not true");

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomeTransactions_UniqueProcessedBlockId",
                table: "NftIncomeTransactions",
                column: "UniqueProcessedBlockId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NftIncomeTransactions");

            migrationBuilder.DropTable(
                name: "UniqueProcessedBlocks");
        }
    }
}
