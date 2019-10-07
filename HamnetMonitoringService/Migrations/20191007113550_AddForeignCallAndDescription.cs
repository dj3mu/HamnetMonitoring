using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable 1591

namespace HamnetDbRest.Migrations
{
    public partial class AddForeignCallAndDescription : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "RssiValues",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ForeignCall",
                table: "RssiValues",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "RssiValues");

            migrationBuilder.DropColumn(
                name: "ForeignCall",
                table: "RssiValues");
        }
    }
}
