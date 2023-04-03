using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DSentBot.Migrations
{
    /// <inheritdoc />
    public partial class DurationMin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationMin",
                table: "Musics",
                type: "INTEGER",
                nullable: false,
                defaultValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DurationMin",
                table: "Musics");
        }
    }
}
