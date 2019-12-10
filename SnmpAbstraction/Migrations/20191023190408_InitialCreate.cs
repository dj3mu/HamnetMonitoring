using System;
using Microsoft.EntityFrameworkCore.Migrations;

#pragma warning disable 1591

namespace SnmpAbstraction.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CacheData",
                columns: table => new
                {
                    DeviceAddress = table.Column<string>(nullable: false),
                    SystemData = table.Column<string>(nullable: true),
                    WirelessPeerInfo = table.Column<string>(nullable: true),
                    InterfaceDetails = table.Column<string>(nullable: true),
                    LastModification = table.Column<DateTime>(nullable: false),
                    ApiUsed = table.Column<int>(nullable: false),
                    DeviceHandlerClass = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CacheData", x => x.DeviceAddress);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CacheData_DeviceAddress",
                table: "CacheData",
                column: "DeviceAddress",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CacheData");
        }
    }
}
