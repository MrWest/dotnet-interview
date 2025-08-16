using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddSynchronizationSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                table: "TodoListItem",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "TodoListItem",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                table: "TodoList",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "TodoList",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETUTCDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_ExternalId",
                table: "TodoListItem",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_IsSynced",
                table: "TodoListItem",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_IsSynced_UpdatedAt",
                table: "TodoListItem",
                columns: new[] { "IsSynced", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_LastSyncedAt",
                table: "TodoListItem",
                column: "LastSyncedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_SourceId",
                table: "TodoListItem",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_TodoListId_IsSynced",
                table: "TodoListItem",
                columns: new[] { "TodoListId", "IsSynced" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoListItem_UpdatedAt",
                table: "TodoListItem",
                column: "UpdatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TodoList_ExternalId",
                table: "TodoList",
                column: "ExternalId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoList_IsSynced",
                table: "TodoList",
                column: "IsSynced");

            migrationBuilder.CreateIndex(
                name: "IX_TodoList_IsSynced_UpdatedAt",
                table: "TodoList",
                columns: new[] { "IsSynced", "UpdatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TodoList_LastSyncedAt",
                table: "TodoList",
                column: "LastSyncedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TodoList_SourceId",
                table: "TodoList",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_TodoList_UpdatedAt",
                table: "TodoList",
                column: "UpdatedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_ExternalId",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_IsSynced",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_IsSynced_UpdatedAt",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_LastSyncedAt",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_SourceId",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_TodoListId_IsSynced",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_UpdatedAt",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoList_ExternalId",
                table: "TodoList");

            migrationBuilder.DropIndex(
                name: "IX_TodoList_IsSynced",
                table: "TodoList");

            migrationBuilder.DropIndex(
                name: "IX_TodoList_IsSynced_UpdatedAt",
                table: "TodoList");

            migrationBuilder.DropIndex(
                name: "IX_TodoList_LastSyncedAt",
                table: "TodoList");

            migrationBuilder.DropIndex(
                name: "IX_TodoList_SourceId",
                table: "TodoList");

            migrationBuilder.DropIndex(
                name: "IX_TodoList_UpdatedAt",
                table: "TodoList");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                table: "TodoListItem",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "TodoListItem",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoListItem",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");

            migrationBuilder.AlterColumn<string>(
                name: "SourceId",
                table: "TodoList",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ExternalId",
                table: "TodoList",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "CreatedAt",
                table: "TodoList",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETUTCDATE()");
        }
    }
}
