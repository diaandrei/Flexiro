using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flexiro.Application.Migrations
{
    /// <inheritdoc />
    public partial class NewMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "01b60732-0d0e-45d2-b754-cac753c2b3ea" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "01b60732-0d0e-45d2-b754-cac753c2b3ea");

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    NotificationType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notification_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "bb23fa6a-6cb3-4f11-92e7-aee34f7fc334", 0, null, null, "d75c028c-87f9-40b5-bfb5-70c8901b026d", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAELrsZkAKe0j5f6UdW4wkawEhkbNOnZRX2Bm1g+HKnGC+UmUpyViDtMaBnJp0rts3fw==", null, false, null, null, "79a1cc12-223e-41ed-bae2-1217306c882f", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "bb23fa6a-6cb3-4f11-92e7-aee34f7fc334" });

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "bb23fa6a-6cb3-4f11-92e7-aee34f7fc334" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "bb23fa6a-6cb3-4f11-92e7-aee34f7fc334");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "01b60732-0d0e-45d2-b754-cac753c2b3ea", 0, null, null, "17ad5341-5827-43b3-a0e4-37c605aa5dba", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAELr5l88LQgc7bU5fxG+yA/ASTvfBo2KuxNo9oneGk4P3Q/lV3BXJPecT29+QFuT96A==", null, false, null, null, "99fc9f17-c3cd-4506-a29f-4985451cfec0", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "01b60732-0d0e-45d2-b754-cac753c2b3ea" });
        }
    }
}
