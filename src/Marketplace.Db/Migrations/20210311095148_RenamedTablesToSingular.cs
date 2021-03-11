using Microsoft.EntityFrameworkCore.Migrations;

namespace Marketplace.Db.Migrations
{
    public partial class RenamedTablesToSingular : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NftIncomeTransactions_UniqueProcessedBlocks_UniqueProcessed~",
                table: "NftIncomeTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Trades_Offers_OfferId",
                table: "Trades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UniqueProcessedBlocks",
                table: "UniqueProcessedBlocks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Trades",
                table: "Trades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuoteOutgoingTransactions",
                table: "QuoteOutgoingTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Offers",
                table: "Offers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NftOutgoingTransactions",
                table: "NftOutgoingTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NftIncomeTransactions",
                table: "NftIncomeTransactions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KusamaProcessedBlocks",
                table: "KusamaProcessedBlocks");

            migrationBuilder.RenameTable(
                name: "UniqueProcessedBlocks",
                newName: "UniqueProcessedBlock");

            migrationBuilder.RenameTable(
                name: "Trades",
                newName: "Trade");

            migrationBuilder.RenameTable(
                name: "QuoteOutgoingTransactions",
                newName: "QuoteOutgoingTransaction");

            migrationBuilder.RenameTable(
                name: "Offers",
                newName: "Offer");

            migrationBuilder.RenameTable(
                name: "NftOutgoingTransactions",
                newName: "NftOutgoingTransaction");

            migrationBuilder.RenameTable(
                name: "NftIncomeTransactions",
                newName: "NftIncomeTransaction");

            migrationBuilder.RenameTable(
                name: "KusamaProcessedBlocks",
                newName: "KusamaProcessedBlock");

            migrationBuilder.RenameIndex(
                name: "IX_Trades_OfferId",
                table: "Trade",
                newName: "IX_Trade_OfferId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteOutgoingTransactions_Status",
                table: "QuoteOutgoingTransaction",
                newName: "IX_QuoteOutgoingTransaction_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Offers_OfferStatus_CollectionId_TokenId",
                table: "Offer",
                newName: "IX_Offer_OfferStatus_CollectionId_TokenId");

            migrationBuilder.RenameIndex(
                name: "IX_Offers_CreationDate",
                table: "Offer",
                newName: "IX_Offer_CreationDate");

            migrationBuilder.RenameIndex(
                name: "IX_NftOutgoingTransactions_Status_LockTime",
                table: "NftOutgoingTransaction",
                newName: "IX_NftOutgoingTransaction_Status_LockTime");

            migrationBuilder.RenameIndex(
                name: "IX_NftIncomeTransactions_UniqueProcessedBlockId",
                table: "NftIncomeTransaction",
                newName: "IX_NftIncomeTransaction_UniqueProcessedBlockId");

            migrationBuilder.RenameIndex(
                name: "IX_NftIncomeTransactions_Status_LockTime",
                table: "NftIncomeTransaction",
                newName: "IX_NftIncomeTransaction_Status_LockTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UniqueProcessedBlock",
                table: "UniqueProcessedBlock",
                column: "BlockNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Trade",
                table: "Trade",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteOutgoingTransaction",
                table: "QuoteOutgoingTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offer",
                table: "Offer",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NftOutgoingTransaction",
                table: "NftOutgoingTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NftIncomeTransaction",
                table: "NftIncomeTransaction",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KusamaProcessedBlock",
                table: "KusamaProcessedBlock",
                column: "BlockNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_NftIncomeTransaction_UniqueProcessedBlock_UniqueProcessedBl~",
                table: "NftIncomeTransaction",
                column: "UniqueProcessedBlockId",
                principalTable: "UniqueProcessedBlock",
                principalColumn: "BlockNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trade_Offer_OfferId",
                table: "Trade",
                column: "OfferId",
                principalTable: "Offer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NftIncomeTransaction_UniqueProcessedBlock_UniqueProcessedBl~",
                table: "NftIncomeTransaction");

            migrationBuilder.DropForeignKey(
                name: "FK_Trade_Offer_OfferId",
                table: "Trade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UniqueProcessedBlock",
                table: "UniqueProcessedBlock");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Trade",
                table: "Trade");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuoteOutgoingTransaction",
                table: "QuoteOutgoingTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Offer",
                table: "Offer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NftOutgoingTransaction",
                table: "NftOutgoingTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_NftIncomeTransaction",
                table: "NftIncomeTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_KusamaProcessedBlock",
                table: "KusamaProcessedBlock");

            migrationBuilder.RenameTable(
                name: "UniqueProcessedBlock",
                newName: "UniqueProcessedBlocks");

            migrationBuilder.RenameTable(
                name: "Trade",
                newName: "Trades");

            migrationBuilder.RenameTable(
                name: "QuoteOutgoingTransaction",
                newName: "QuoteOutgoingTransactions");

            migrationBuilder.RenameTable(
                name: "Offer",
                newName: "Offers");

            migrationBuilder.RenameTable(
                name: "NftOutgoingTransaction",
                newName: "NftOutgoingTransactions");

            migrationBuilder.RenameTable(
                name: "NftIncomeTransaction",
                newName: "NftIncomeTransactions");

            migrationBuilder.RenameTable(
                name: "KusamaProcessedBlock",
                newName: "KusamaProcessedBlocks");

            migrationBuilder.RenameIndex(
                name: "IX_Trade_OfferId",
                table: "Trades",
                newName: "IX_Trades_OfferId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteOutgoingTransaction_Status",
                table: "QuoteOutgoingTransactions",
                newName: "IX_QuoteOutgoingTransactions_Status");

            migrationBuilder.RenameIndex(
                name: "IX_Offer_OfferStatus_CollectionId_TokenId",
                table: "Offers",
                newName: "IX_Offers_OfferStatus_CollectionId_TokenId");

            migrationBuilder.RenameIndex(
                name: "IX_Offer_CreationDate",
                table: "Offers",
                newName: "IX_Offers_CreationDate");

            migrationBuilder.RenameIndex(
                name: "IX_NftOutgoingTransaction_Status_LockTime",
                table: "NftOutgoingTransactions",
                newName: "IX_NftOutgoingTransactions_Status_LockTime");

            migrationBuilder.RenameIndex(
                name: "IX_NftIncomeTransaction_UniqueProcessedBlockId",
                table: "NftIncomeTransactions",
                newName: "IX_NftIncomeTransactions_UniqueProcessedBlockId");

            migrationBuilder.RenameIndex(
                name: "IX_NftIncomeTransaction_Status_LockTime",
                table: "NftIncomeTransactions",
                newName: "IX_NftIncomeTransactions_Status_LockTime");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UniqueProcessedBlocks",
                table: "UniqueProcessedBlocks",
                column: "BlockNumber");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Trades",
                table: "Trades",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteOutgoingTransactions",
                table: "QuoteOutgoingTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Offers",
                table: "Offers",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NftOutgoingTransactions",
                table: "NftOutgoingTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_NftIncomeTransactions",
                table: "NftIncomeTransactions",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_KusamaProcessedBlocks",
                table: "KusamaProcessedBlocks",
                column: "BlockNumber");

            migrationBuilder.AddForeignKey(
                name: "FK_NftIncomeTransactions_UniqueProcessedBlocks_UniqueProcessed~",
                table: "NftIncomeTransactions",
                column: "UniqueProcessedBlockId",
                principalTable: "UniqueProcessedBlocks",
                principalColumn: "BlockNumber",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Trades_Offers_OfferId",
                table: "Trades",
                column: "OfferId",
                principalTable: "Offers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
