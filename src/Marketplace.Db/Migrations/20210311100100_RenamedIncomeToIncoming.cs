using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class RenamedIncomeToIncoming : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NftIncomeTransaction");

            migrationBuilder.DropTable(
                name: "QuoteIncomeTransactions");

            migrationBuilder.CreateTable(
                name: "NftIncomingTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<long>(type: "bigint", nullable: false),
                    TokenId = table.Column<long>(type: "bigint", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    OwnerPublicKey = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LockTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    UniqueProcessedBlockId = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NftIncomingTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NftIncomingTransaction_UniqueProcessedBlock_UniqueProcessed~",
                        column: x => x.UniqueProcessedBlockId,
                        principalTable: "UniqueProcessedBlock",
                        principalColumn: "BlockNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuoteIncomingTransaction",
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
                    table.PrimaryKey("PK_QuoteIncomingTransaction", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomingTransaction_Status_LockTime",
                table: "NftIncomingTransaction",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomingTransaction_UniqueProcessedBlockId",
                table: "NftIncomingTransaction",
                column: "UniqueProcessedBlockId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteIncomingTransaction_AccountPublicKey",
                table: "QuoteIncomingTransaction",
                column: "AccountPublicKey");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteIncomingTransaction_Status_LockTime",
                table: "QuoteIncomingTransaction",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NftIncomingTransaction");

            migrationBuilder.DropTable(
                name: "QuoteIncomingTransaction");

            migrationBuilder.CreateTable(
                name: "NftIncomeTransaction",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CollectionId = table.Column<long>(type: "bigint", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    LockTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    OwnerPublicKey = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TokenId = table.Column<long>(type: "bigint", nullable: false),
                    UniqueProcessedBlockId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NftIncomeTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NftIncomeTransaction_UniqueProcessedBlock_UniqueProcessedBl~",
                        column: x => x.UniqueProcessedBlockId,
                        principalTable: "UniqueProcessedBlock",
                        principalColumn: "BlockNumber",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuoteIncomeTransactions",
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
                    table.PrimaryKey("PK_QuoteIncomeTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomeTransaction_Status_LockTime",
                table: "NftIncomeTransaction",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_NftIncomeTransaction_UniqueProcessedBlockId",
                table: "NftIncomeTransaction",
                column: "UniqueProcessedBlockId");

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
    }
}
