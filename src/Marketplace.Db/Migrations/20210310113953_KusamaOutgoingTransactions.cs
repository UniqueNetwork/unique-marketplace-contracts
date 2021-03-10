using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class KusamaOutgoingTransactions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KusamaTransactions");

            migrationBuilder.CreateTable(
                name: "KusamaIncomeTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    AccountPublicKey = table.Column<string>(type: "text", nullable: false),
                    BlockId = table.Column<decimal>(type: "numeric(20,0)", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    LockTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KusamaIncomeTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KusamaOutgoingTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    Value = table.Column<string>(type: "text", nullable: false),
                    QuoteId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    RecipientPublicKey = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KusamaOutgoingTransactions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KusamaIncomeTransactions_AccountPublicKey",
                table: "KusamaIncomeTransactions",
                column: "AccountPublicKey");

            migrationBuilder.CreateIndex(
                name: "IX_KusamaIncomeTransactions_Status_LockTime",
                table: "KusamaIncomeTransactions",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_KusamaOutgoingTransactions_Status",
                table: "KusamaOutgoingTransactions",
                column: "Status",
                filter: "\"Status\" = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KusamaIncomeTransactions");

            migrationBuilder.DropTable(
                name: "KusamaOutgoingTransactions");

            migrationBuilder.CreateTable(
                name: "KusamaTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountPublicKey = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<string>(type: "text", nullable: false),
                    BlockId = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    LockTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_KusamaTransactions_Status_LockTime",
                table: "KusamaTransactions",
                columns: new[] { "Status", "LockTime" },
                filter: "\"Status\" = 0");
        }
    }
}
