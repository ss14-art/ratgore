using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class RatFactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "rat_faction",
                columns: table => new
                {
                    rat_faction_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: false),
                    is_whitelisted = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rat_faction", x => x.rat_faction_id);
                });

            migrationBuilder.CreateTable(
                name: "rat_faction_manager",
                columns: table => new
                {
                    player_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    faction_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rat_faction_manager", x => new { x.player_user_id, x.faction_id });
                    table.ForeignKey(
                        name: "FK_rat_faction_manager_player_player_user_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rat_faction_manager_rat_faction_faction_id",
                        column: x => x.faction_id,
                        principalTable: "rat_faction",
                        principalColumn: "rat_faction_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rat_faction_whitelist",
                columns: table => new
                {
                    player_user_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    faction_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_rat_faction_whitelist", x => new { x.player_user_id, x.faction_id });
                    table.ForeignKey(
                        name: "FK_rat_faction_whitelist_player_player_user_id",
                        column: x => x.player_user_id,
                        principalTable: "player",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_rat_faction_whitelist_rat_faction_faction_id",
                        column: x => x.faction_id,
                        principalTable: "rat_faction",
                        principalColumn: "rat_faction_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_rat_faction_name",
                table: "rat_faction",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_rat_faction_manager_faction_id",
                table: "rat_faction_manager",
                column: "faction_id");

            migrationBuilder.CreateIndex(
                name: "IX_rat_faction_whitelist_faction_id",
                table: "rat_faction_whitelist",
                column: "faction_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "rat_faction_manager");

            migrationBuilder.DropTable(
                name: "rat_faction_whitelist");

            migrationBuilder.DropTable(
                name: "rat_faction");
        }
    }
}
