using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BettingDataProvider.Migrations
{
    /// <inheritdoc />
    public partial class OneToManyRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Matches_MatchId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Sports_SportId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Events_EventId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Odds_Bets_BetId",
                table: "Odds");

            migrationBuilder.AlterColumn<int>(
                name: "BetId",
                table: "Odds",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EventId",
                table: "Matches",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SportId",
                table: "Events",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MatchId",
                table: "Bets",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Matches_MatchId",
                table: "Bets",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Sports_SportId",
                table: "Events",
                column: "SportId",
                principalTable: "Sports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Events_EventId",
                table: "Matches",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Odds_Bets_BetId",
                table: "Odds",
                column: "BetId",
                principalTable: "Bets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bets_Matches_MatchId",
                table: "Bets");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Sports_SportId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Matches_Events_EventId",
                table: "Matches");

            migrationBuilder.DropForeignKey(
                name: "FK_Odds_Bets_BetId",
                table: "Odds");

            migrationBuilder.AlterColumn<int>(
                name: "BetId",
                table: "Odds",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EventId",
                table: "Matches",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "SportId",
                table: "Events",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "MatchId",
                table: "Bets",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Bets_Matches_MatchId",
                table: "Bets",
                column: "MatchId",
                principalTable: "Matches",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Sports_SportId",
                table: "Events",
                column: "SportId",
                principalTable: "Sports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Matches_Events_EventId",
                table: "Matches",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Odds_Bets_BetId",
                table: "Odds",
                column: "BetId",
                principalTable: "Bets",
                principalColumn: "Id");
        }
    }
}
