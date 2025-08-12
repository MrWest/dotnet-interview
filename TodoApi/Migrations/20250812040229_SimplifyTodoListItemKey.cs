using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class SimplifyTodoListItemKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListItems_TodoList_TodoListId",
                table: "TodoListItems");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoListItems",
                table: "TodoListItems");

            migrationBuilder.RenameTable(
                name: "TodoListItems",
                newName: "TodoListItem");

            migrationBuilder.RenameIndex(
                name: "IX_TodoListItems_TodoListId",
                table: "TodoListItem",
                newName: "IX_TodoListItem_TodoListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoListItem",
                table: "TodoListItem",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoListItem_TodoList_TodoListId",
                table: "TodoListItem",
                column: "TodoListId",
                principalTable: "TodoList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListItem_TodoList_TodoListId",
                table: "TodoListItem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoListItem",
                table: "TodoListItem");

            migrationBuilder.RenameTable(
                name: "TodoListItem",
                newName: "TodoListItems");

            migrationBuilder.RenameIndex(
                name: "IX_TodoListItem_TodoListId",
                table: "TodoListItems",
                newName: "IX_TodoListItems_TodoListId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoListItems",
                table: "TodoListItems",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoListItems_TodoList_TodoListId",
                table: "TodoListItems",
                column: "TodoListId",
                principalTable: "TodoList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
