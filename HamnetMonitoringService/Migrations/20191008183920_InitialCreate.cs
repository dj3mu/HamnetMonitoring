using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable 1591

namespace HamnetDbRest.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MonitoringStatus",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LastQueryStart = table.Column<DateTime>(nullable: false),
                    LastQueryEnd = table.Column<DateTime>(nullable: false),
                    LastMaintenanceStart = table.Column<DateTime>(nullable: false),
                    LastMaintenanceEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MonitoringStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RssiFailingQueries",
                columns: table => new
                {
                    Subnet = table.Column<string>(nullable: false),
                    TimeStamp = table.Column<DateTime>(nullable: false),
                    ErrorInfo = table.Column<string>(nullable: false),
                    AffectedHosts = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RssiFailingQueries", x => x.Subnet);
                });

            migrationBuilder.CreateTable(
                name: "RssiValues",
                columns: table => new
                {
                    ForeignId = table.Column<string>(nullable: false),
                    MetricId = table.Column<int>(nullable: false),
                    Metric = table.Column<string>(nullable: false),
                    UnixTimeStamp = table.Column<ulong>(nullable: false),
                    TimeStampString = table.Column<string>(nullable: false),
                    RssiValue = table.Column<string>(nullable: false),
                    ParentSubnet = table.Column<string>(nullable: false),
                    ForeignCall = table.Column<string>(nullable: false),
                    Description = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RssiValues", x => x.ForeignId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RssiValues_ForeignId",
                table: "RssiValues",
                column: "ForeignId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MonitoringStatus");

            migrationBuilder.DropTable(
                name: "RssiFailingQueries");

            migrationBuilder.DropTable(
                name: "RssiValues");
        }
    }
}
