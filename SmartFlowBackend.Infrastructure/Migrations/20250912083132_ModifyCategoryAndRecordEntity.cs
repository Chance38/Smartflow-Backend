using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlowBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifyCategoryAndRecordEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Record",
                newName: "CategoryType");

            migrationBuilder.RenameColumn(
                name: "Type",
                table: "Category",
                newName: "CategoryType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CategoryType",
                table: "Record",
                newName: "Type");

            migrationBuilder.RenameColumn(
                name: "CategoryType",
                table: "Category",
                newName: "Type");
        }
    }
}
