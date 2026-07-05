using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TableTop.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "MapTokens",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "MapTokens");
        }
    }
}
