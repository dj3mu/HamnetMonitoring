using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable 1591

namespace HamnetDbRest.Migrations
{
    public partial class AddBgpMonitoringTimestampColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TimeStampString",
                table: "BgpPeers",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<ulong>(
                name: "UnixTimeStamp",
                table: "BgpPeers",
                nullable: false,
                defaultValue: 0ul);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeStampString",
                table: "BgpPeers");

            migrationBuilder.DropColumn(
                name: "UnixTimeStamp",
                table: "BgpPeers");
        }
    }
}
