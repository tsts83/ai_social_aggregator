using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialAggregatorAPI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNewsArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Url",
                table: "NewsArticles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Url",
                table: "NewsArticles");
        }
    }
}
