using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable 1591

namespace HamnetDbRest.Migrations
{
    public partial class PenaltyInfo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PenaltyInfo",
                table: "BgpFailingQueries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PenaltyInfo",
                table: "BgpFailingQueries");
        }
    }
}
