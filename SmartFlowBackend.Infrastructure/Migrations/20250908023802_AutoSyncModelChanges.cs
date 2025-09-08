using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlowBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AutoSyncModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecordTag_Record_RecordsId",
                table: "RecordTag");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordTag_Tag_TagsId",
                table: "RecordTag");

            migrationBuilder.DropForeignKey(
                name: "FK_Tag_Category_CategoryId",
                table: "Tag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.DropIndex(
                name: "IX_Tag_CategoryId",
                table: "Tag");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Tag");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "User",
                newName: "UserPassword");

            migrationBuilder.RenameColumn(
                name: "Account",
                table: "User",
                newName: "UserAccount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "User",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Tag",
                newName: "TagName");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Tag",
                newName: "TagId");

            migrationBuilder.RenameColumn(
                name: "TagsId",
                table: "RecordTag",
                newName: "TagsTagId");

            migrationBuilder.RenameColumn(
                name: "RecordsId",
                table: "RecordTag",
                newName: "RecordsRecordId");

            migrationBuilder.RenameIndex(
                name: "IX_RecordTag_TagsId",
                table: "RecordTag",
                newName: "IX_RecordTag_TagsTagId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Record",
                newName: "RecordId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "MonthlySummary",
                newName: "MonthlySummaryId");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Category",
                newName: "CategoryName");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Category",
                newName: "CategoryId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_RecordTag_Record_RecordsRecordId",
                table: "RecordTag",
                column: "RecordsRecordId",
                principalTable: "Record",
                principalColumn: "RecordId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RecordTag_Tag_TagsTagId",
                table: "RecordTag",
                column: "TagsTagId",
                principalTable: "Tag",
                principalColumn: "TagId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RecordTag_Record_RecordsRecordId",
                table: "RecordTag");

            migrationBuilder.DropForeignKey(
                name: "FK_RecordTag_Tag_TagsTagId",
                table: "RecordTag");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Tag",
                table: "Tag");

            migrationBuilder.RenameColumn(
                name: "UserPassword",
                table: "User",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "UserAccount",
                table: "User",
                newName: "Account");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "User",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "TagName",
                table: "Tag",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "TagId",
                table: "Tag",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "TagsTagId",
                table: "RecordTag",
                newName: "TagsId");

            migrationBuilder.RenameColumn(
                name: "RecordsRecordId",
                table: "RecordTag",
                newName: "RecordsId");

            migrationBuilder.RenameIndex(
                name: "IX_RecordTag_TagsTagId",
                table: "RecordTag",
                newName: "IX_RecordTag_TagsId");

            migrationBuilder.RenameColumn(
                name: "RecordId",
                table: "Record",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "MonthlySummaryId",
                table: "MonthlySummary",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "Category",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Category",
                newName: "Id");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "Tag",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_Tag",
                table: "Tag",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Tag_CategoryId",
                table: "Tag",
                column: "CategoryId");

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
        }
    }
}
