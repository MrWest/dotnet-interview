using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class SycnItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "TodoListItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "TodoListItem",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "TodoListItem",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "TodoListItemId",
                table: "TodoListItem",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "TodoList",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSynced",
                table: "TodoList",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSyncedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceId",
                table: "TodoList",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_TodoListItemId",
                table: "TodoListItem",
                column: "TodoListItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoListItem_TodoListItem_TodoListItemId",
                table: "TodoListItem",
                column: "TodoListItemId",
                principalTable: "TodoListItem",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListItem_TodoListItem_TodoListItemId",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_TodoListItemId",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "TodoListItemId",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "IsSynced",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "LastSyncedAt",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "SourceId",
                table: "TodoList");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "TodoList");
        }
    }
}
