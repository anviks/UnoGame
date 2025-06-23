using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerCardRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerCard_Cards_CardId",
                table: "PlayerCard");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerCard_Players_PlayerId",
                table: "PlayerCard");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerCard",
                table: "PlayerCard");

            migrationBuilder.RenameTable(
                name: "PlayerCard",
                newName: "PlayerCards");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerCard_PlayerId",
                table: "PlayerCards",
                newName: "IX_PlayerCards_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerCard_CardId",
                table: "PlayerCards",
                newName: "IX_PlayerCards_CardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerCards",
                table: "PlayerCards",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerCards_Cards_CardId",
                table: "PlayerCards",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerCards_Players_PlayerId",
                table: "PlayerCards",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerCards_Cards_CardId",
                table: "PlayerCards");

            migrationBuilder.DropForeignKey(
                name: "FK_PlayerCards_Players_PlayerId",
                table: "PlayerCards");

            migrationBuilder.DropPrimaryKey(
                name: "PK_PlayerCards",
                table: "PlayerCards");

            migrationBuilder.RenameTable(
                name: "PlayerCards",
                newName: "PlayerCard");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerCards_PlayerId",
                table: "PlayerCard",
                newName: "IX_PlayerCard_PlayerId");

            migrationBuilder.RenameIndex(
                name: "IX_PlayerCards_CardId",
                table: "PlayerCard",
                newName: "IX_PlayerCard_CardId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_PlayerCard",
                table: "PlayerCard",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerCard_Cards_CardId",
                table: "PlayerCard",
                column: "CardId",
                principalTable: "Cards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerCard_Players_PlayerId",
                table: "PlayerCard",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
