using Microsoft.EntityFrameworkCore.Migrations;

namespace MyVet.Web.Migrations
{
    public partial class completeDBFinaly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceTypeId",
                table: "ServiceTypes",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceTypes_ServiceTypeId",
                table: "ServiceTypes",
                column: "ServiceTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceTypes_ServiceTypes_ServiceTypeId",
                table: "ServiceTypes",
                column: "ServiceTypeId",
                principalTable: "ServiceTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceTypes_ServiceTypes_ServiceTypeId",
                table: "ServiceTypes");

            migrationBuilder.DropIndex(
                name: "IX_ServiceTypes_ServiceTypeId",
                table: "ServiceTypes");

            migrationBuilder.DropColumn(
                name: "ServiceTypeId",
                table: "ServiceTypes");
        }
    }
}
