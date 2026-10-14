using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hirfolyam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyHotTopics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Day",
                table: "HotTopicSnapshots",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HotTopicSnapshots_Day",
                table: "HotTopicSnapshots",
                column: "Day");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HotTopicSnapshots_Day",
                table: "HotTopicSnapshots");

            migrationBuilder.DropColumn(
                name: "Day",
                table: "HotTopicSnapshots");
        }
    }
}
