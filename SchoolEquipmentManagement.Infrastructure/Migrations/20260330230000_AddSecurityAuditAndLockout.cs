using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SchoolEquipmentManagement.Infrastructure.Data;

#nullable disable

namespace SchoolEquipmentManagement.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260330230000_AddSecurityAuditAndLockout")]
    public partial class AddSecurityAuditAndLockout : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FailedSignInAttempts",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastSignInAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndUtc",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SecurityAuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    TargetUserName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAuditEntries", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEntries_EventType",
                table: "SecurityAuditEntries",
                column: "EventType");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEntries_IsSuccessful",
                table: "SecurityAuditEntries",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "IX_SecurityAuditEntries_OccurredAt",
                table: "SecurityAuditEntries",
                column: "OccurredAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityAuditEntries");

            migrationBuilder.DropColumn(
                name: "FailedSignInAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastSignInAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEndUtc",
                table: "Users");
        }
    }
}
