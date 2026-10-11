using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hirfolyam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RenameArticleLeadText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LeadSearchText",
                table: "Articles",
                newName: "LeadText");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LeadText",
                table: "Articles",
                newName: "LeadSearchText");
        }
    }
}
