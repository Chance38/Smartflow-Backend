using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlowBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UseSingularTableNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Users_UserId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlySummaries_Users_UserId",
                table: "MonthlySummaries");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_Categories_CategoryId",
                table: "Records");

            migrationBuilder.DropForeignKey(
                name: "FK_Records_Users_UserId",
                table: "Records");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordTag_Records_RecordsId",
                table: "RecordTag");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordTag_Tags_TagsId",
                table: "RecordTag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Categories_CategoryId",
                table: "Tags");

            migrationBuilder.DropForeignKey(
                name: "FK_Tags_Users_UserId",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Users",
                table: "Users");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tags",
                table: "Tags");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Records",
                table: "Records");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MonthlySummaries",
                table: "MonthlySummaries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Categories",
                table: "Categories");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "User");

            migrationBuilder.RenameTable(
                name: "Tags",
                newName: "Tag");

            migrationBuilder.RenameTable(
                name: "Records",
                newName: "Record");

            migrationBuilder.RenameTable(
                name: "MonthlySummaries",
                newName: "MonthlySummary");

            migrationBuilder.RenameTable(
                name: "Categories",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_UserId",
                table: "Tag",
                newName: "IX_Tag_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tags_CategoryId",
                table: "Tag",
                newName: "IX_Tag_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Records_UserId",
                table: "Record",
                newName: "IX_Record_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Records_Date",
                table: "Record",
                newName: "IX_Record_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Records_CategoryId",
                table: "Record",
                newName: "IX_Record_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MonthlySummaries_UserId",
                table: "MonthlySummary",
                newName: "IX_MonthlySummary_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Categories_UserId",
                table: "Category",
                newName: "IX_Category_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_User",
                table: "User",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Record",
                table: "Record",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonthlySummary",
                table: "MonthlySummary",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Category",
                table: "Category",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_User_UserId",
                table: "Category",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlySummary_User_UserId",
                table: "MonthlySummary",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Record_Category_CategoryId",
                table: "Record",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Record_User_UserId",
                table: "Record",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordTag_Record_RecordsId",
                table: "RecordTag",
                column: "RecordsId",
                principalTable: "Record",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordTag_Tag_TagsId",
                table: "RecordTag",
                column: "TagsId",
                principalTable: "Tag",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_Category_CategoryId",
                table: "Tag",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tag_User_UserId",
                table: "Tag",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_User_UserId",
                table: "Category");

            migrationBuilder.DropForeignKey(
                name: "FK_MonthlySummary_User_UserId",
                table: "MonthlySummary");

            migrationBuilder.DropForeignKey(
                name: "FK_Record_Category_CategoryId",
                table: "Record");

            migrationBuilder.DropForeignKey(
                name: "FK_Record_User_UserId",
                table: "Record");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordTag_Record_RecordsId",
                table: "RecordTag");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordTag_Tag_TagsId",
                table: "RecordTag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Category_CategoryId",
                table: "Tag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_User_UserId",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_User",
                table: "User");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Record",
                table: "Record");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MonthlySummary",
                table: "MonthlySummary");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Category",
                table: "Category");

            migrationBuilder.RenameTable(
                name: "User",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Tag",
                newName: "Tags");

            migrationBuilder.RenameTable(
                name: "Record",
                newName: "Records");

            migrationBuilder.RenameTable(
                name: "MonthlySummary",
                newName: "MonthlySummaries");

            migrationBuilder.RenameTable(
                name: "Category",
                newName: "Categories");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_UserId",
                table: "Tags",
                newName: "IX_Tags_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Tag_CategoryId",
                table: "Tags",
                newName: "IX_Tags_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Record_UserId",
                table: "Records",
                newName: "IX_Records_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Record_Date",
                table: "Records",
                newName: "IX_Records_Date");

            migrationBuilder.RenameIndex(
                name: "IX_Record_CategoryId",
                table: "Records",
                newName: "IX_Records_CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_MonthlySummary_UserId",
                table: "MonthlySummaries",
                newName: "IX_MonthlySummaries_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Category_UserId",
                table: "Categories",
                newName: "IX_Categories_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Users",
                table: "Users",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tags",
                table: "Tags",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Records",
                table: "Records",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MonthlySummaries",
                table: "MonthlySummaries",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Categories",
                table: "Categories",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Users_UserId",
                table: "Categories",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MonthlySummaries_Users_UserId",
                table: "MonthlySummaries",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Categories_CategoryId",
                table: "Records",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Records_Users_UserId",
                table: "Records",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordTag_Records_RecordsId",
                table: "RecordTag",
                column: "RecordsId",
                principalTable: "Records",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordTag_Tags_TagsId",
                table: "RecordTag",
                column: "TagsId",
                principalTable: "Tags",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Categories_CategoryId",
                table: "Tags",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tags_Users_UserId",
                table: "Tags",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
