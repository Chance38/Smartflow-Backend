using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartFlowBackend.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NullableCategoryInRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Record_Category_CategoryId",
                table: "Record");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Record",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "Record",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<List<string>>(
                name: "TagNames",
                table: "Record",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Record_Category_CategoryId",
                table: "Record",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Record_Category_CategoryId",
                table: "Record");

            migrationBuilder.DropColumn(
                name: "CategoryName",
                table: "Record");

            migrationBuilder.DropColumn(
                name: "TagNames",
                table: "Record");

            migrationBuilder.AlterColumn<Guid>(
                name: "CategoryId",
                table: "Record",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Record_Category_CategoryId",
                table: "Record",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
