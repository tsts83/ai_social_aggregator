﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SocialAggregatorAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddThumbnailUrlToNewsArticles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "NewsArticles",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "NewsArticles");
        }
    }
}
