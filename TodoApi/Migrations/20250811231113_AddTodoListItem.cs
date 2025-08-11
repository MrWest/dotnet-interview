using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoListItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListItems_TodoList_TodoListId",
                table: "TodoListItems");

            migrationBuilder.AlterColumn<long>(
                name: "TodoListId",
                table: "TodoListItems",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TodoListItems_TodoList_TodoListId",
                table: "TodoListItems",
                column: "TodoListId",
                principalTable: "TodoList",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListItems_TodoList_TodoListId",
                table: "TodoListItems");

            migrationBuilder.AlterColumn<long>(
                name: "TodoListId",
                table: "TodoListItems",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddForeignKey(
                name: "FK_TodoListItems_TodoList_TodoListId",
                table: "TodoListItems",
                column: "TodoListId",
                principalTable: "TodoList",
                principalColumn: "Id");
        }
    }
}
