using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Flexiro.Application.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", null, "Admin", "ADMIN" },
                    { "2", null, "Seller", "SELLER" },
                    { "3", null, "Customer", "CUSTOMER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "01b60732-0d0e-45d2-b754-cac753c2b3ea", 0, null, null, "17ad5341-5827-43b3-a0e4-37c605aa5dba", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAELr5l88LQgc7bU5fxG+yA/ASTvfBo2KuxNo9oneGk4P3Q/lV3BXJPecT29+QFuT96A==", null, false, null, null, "99fc9f17-c3cd-4506-a29f-4985451cfec0", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "01b60732-0d0e-45d2-b754-cac753c2b3ea" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "2");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "3");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "01b60732-0d0e-45d2-b754-cac753c2b3ea" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "01b60732-0d0e-45d2-b754-cac753c2b3ea");
        }
    }
}
