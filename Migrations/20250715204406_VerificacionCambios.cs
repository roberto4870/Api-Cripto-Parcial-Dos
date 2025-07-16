using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ApiCriptoParcialI.Migrations
{
    /// <inheritdoc />
    public partial class VerificacionCambios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Clientes_ClientId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ClientId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Transactions");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ClienteId",
                table: "Transactions",
                column: "ClienteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Clientes_ClienteId",
                table: "Transactions",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Clientes_ClienteId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ClienteId",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ClientId",
                table: "Transactions",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Clientes_ClientId",
                table: "Transactions",
                column: "ClientId",
                principalTable: "Clientes",
                principalColumn: "Id");
        }
    }
}
