using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable 1591

namespace HamnetDbRest.Migrations
{
    public partial class AddBgpMonitoring : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LastQueryStart",
                table: "MonitoringStatus",
                newName: "LastRssiQueryStart");

            migrationBuilder.RenameColumn(
                name: "LastQueryEnd",
                table: "MonitoringStatus",
                newName: "LastRssiQueryEnd");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastBgpQueryEnd",
                table: "MonitoringStatus",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "LastBgpQueryStart",
                table: "MonitoringStatus",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "BgpFailingQueries",
                columns: table => new
                {
                    Host = table.Column<string>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    ErrorInfo = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BgpFailingQueries", x => x.Host);
                });

            migrationBuilder.CreateTable(
                name: "BgpPeers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RemoteAddress = table.Column<string>(nullable: false),
                    LocalAddress = table.Column<string>(nullable: false),
                    PeeringName = table.Column<string>(nullable: true),
                    Uptime = table.Column<string>(nullable: true),
                    PrefixCount = table.Column<long>(nullable: false),
                    PeeringState = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BgpPeers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BgpPeers_RemoteAddress_LocalAddress",
                table: "BgpPeers",
                columns: new[] { "RemoteAddress", "LocalAddress" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BgpFailingQueries");

            migrationBuilder.DropTable(
                name: "BgpPeers");

            migrationBuilder.DropColumn(
                name: "LastBgpQueryEnd",
                table: "MonitoringStatus");

            migrationBuilder.DropColumn(
                name: "LastBgpQueryStart",
                table: "MonitoringStatus");

            migrationBuilder.RenameColumn(
                name: "LastRssiQueryStart",
                table: "MonitoringStatus",
                newName: "LastQueryStart");

            migrationBuilder.RenameColumn(
                name: "LastRssiQueryEnd",
                table: "MonitoringStatus",
                newName: "LastQueryEnd");
        }
    }
}
