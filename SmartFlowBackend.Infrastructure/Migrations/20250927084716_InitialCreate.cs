using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlowBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Balance",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Balance", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MonthlySummary",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Month = table.Column<int>(type: "integer", nullable: false),
                    Income = table.Column<float>(type: "real", nullable: false),
                    Expense = table.Column<float>(type: "real", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonthlySummary", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RecordTemplate",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: true),
                    CategoryType = table.Column<string>(type: "text", nullable: true),
                    TagNames = table.Column<List<string>>(type: "text[]", nullable: false),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordTemplate", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tag",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Record",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<float>(type: "real", nullable: false),
                    CategoryName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    TagNames = table.Column<List<string>>(type: "text[]", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Record", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Record_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "RecordTag",
                columns: table => new
                {
                    RecordsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TagsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecordTag", x => new { x.RecordsId, x.TagsId });
                    table.ForeignKey(
                        name: "FK_RecordTag_Record_RecordsId",
                        column: x => x.RecordsId,
                        principalTable: "Record",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecordTag_Tag_TagsId",
                        column: x => x.TagsId,
                        principalTable: "Tag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Balance_UserId",
                table: "Balance",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_UserId",
                table: "Category",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_MonthlySummary_UserId",
                table: "MonthlySummary",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Record_CategoryId",
                table: "Record",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Record_CategoryName",
                table: "Record",
                column: "CategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_Record_Date",
                table: "Record",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Record_TagNames",
                table: "Record",
                column: "TagNames");

            migrationBuilder.CreateIndex(
                name: "IX_Record_UserId",
                table: "Record",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordTag_TagsId",
                table: "RecordTag",
                column: "TagsId");

            migrationBuilder.CreateIndex(
                name: "IX_RecordTemplate_Name",
                table: "RecordTemplate",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_RecordTemplate_UserId",
                table: "RecordTemplate",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_UserId",
                table: "Tag",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Balance");

            migrationBuilder.DropTable(
                name: "MonthlySummary");

            migrationBuilder.DropTable(
                name: "RecordTag");

            migrationBuilder.DropTable(
                name: "RecordTemplate");

            migrationBuilder.DropTable(
                name: "Record");

            migrationBuilder.DropTable(
                name: "Tag");

            migrationBuilder.DropTable(
                name: "Category");
        }
    }
}
