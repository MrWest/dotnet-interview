using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoApi.Migrations
{
    /// <inheritdoc />
    public partial class SycnItemCorrected : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TodoListItem_TodoListItem_TodoListItemId",
                table: "TodoListItem");

            migrationBuilder.DropIndex(
                name: "IX_TodoListItem_TodoListItemId",
                table: "TodoListItem");

            migrationBuilder.DropColumn(
                name: "TodoListItemId",
                table: "TodoListItem");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "TodoListItemId",
                table: "TodoListItem",
                type: "bigint",
                nullable: true);

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
    }
}
