using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Hirfolyam.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Slug = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HotTopicSnapshots",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ArticleCount = table.Column<int>(type: "integer", nullable: false),
                    InputTokens = table.Column<int>(type: "integer", nullable: true),
                    OutputTokens = table.Column<int>(type: "integer", nullable: true),
                    CostUsd = table.Column<decimal>(type: "numeric(18,10)", precision: 18, scale: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotTopicSnapshots", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BaseUrl = table.Column<string>(type: "text", nullable: false),
                    IconUrl = table.Column<string>(type: "text", nullable: true),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastImportedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsSources", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NewsSources_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HotTopics",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SnapshotId = table.Column<long>(type: "bigint", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ArticleIds = table.Column<long[]>(type: "bigint[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HotTopics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HotTopics_HotTopicSnapshots_SnapshotId",
                        column: x => x.SnapshotId,
                        principalTable: "HotTopicSnapshots",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NewsSourceId = table.Column<int>(type: "integer", nullable: false),
                    CategoryId = table.Column<int>(type: "integer", nullable: true),
                    ExternalId = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Lead = table.Column<string>(type: "text", nullable: true),
                    LeadSearchText = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: true),
                    Author = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    PublishedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExternalIdHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    UrlHash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Articles_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Articles_NewsSources_NewsSourceId",
                        column: x => x.NewsSourceId,
                        principalTable: "NewsSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "Name", "Slug" },
                values: new object[,]
                {
                    { 1, "Politika", "politics" },
                    { 2, "Gazdaság", "economy" },
                    { 3, "Külföld", "world" },
                    { 4, "Belföld", "domestic" },
                    { 5, "Kultúra", "culture" },
                    { 6, "Tudomány", "science-tech" },
                    { 7, "Sport", "sport" },
                    { 8, "Bulvár", "tabloid" },
                    { 9, "Életmód", "lifestyle" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_CategoryId_PublishedAt",
                table: "Articles",
                columns: new[] { "CategoryId", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_NewsSourceId_PublishedAt",
                table: "Articles",
                columns: new[] { "NewsSourceId", "PublishedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Articles_PublishedAt_Id",
                table: "Articles",
                columns: new[] { "PublishedAt", "Id" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "UX_Articles_Source_ExternalId",
                table: "Articles",
                columns: new[] { "NewsSourceId", "ExternalIdHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_Articles_Source_Url",
                table: "Articles",
                columns: new[] { "NewsSourceId", "UrlHash" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HotTopics_SnapshotId_Rank",
                table: "HotTopics",
                columns: new[] { "SnapshotId", "Rank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HotTopicSnapshots_GeneratedAt",
                table: "HotTopicSnapshots",
                column: "GeneratedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NewsSources_CategoryId",
                table: "NewsSources",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_NewsSources_Key",
                table: "NewsSources",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "HotTopics");

            migrationBuilder.DropTable(
                name: "NewsSources");

            migrationBuilder.DropTable(
                name: "HotTopicSnapshots");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
