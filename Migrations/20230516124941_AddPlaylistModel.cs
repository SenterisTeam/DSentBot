using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSentBot.Migrations
{
    /// <inheritdoc />
    public partial class AddPlaylistModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<ulong>(
                name: "PlaylistId",
                table: "Musics",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", nullable: false),
                    DiscordId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    AuthorId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_AuthorId",
                        column: x => x.AuthorId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Musics_PlaylistId",
                table: "Musics",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_AuthorId",
                table: "Playlists",
                column: "AuthorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Musics_Playlists_PlaylistId",
                table: "Musics",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Musics_Playlists_PlaylistId",
                table: "Musics");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Musics_PlaylistId",
                table: "Musics");

            migrationBuilder.DropColumn(
                name: "PlaylistId",
                table: "Musics");
        }
    }
}
