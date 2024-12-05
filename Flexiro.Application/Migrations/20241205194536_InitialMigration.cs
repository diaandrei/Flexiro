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
                keyValues: new object[] { "1", "7efcac2e-3363-4cb1-8a68-5b0f30def307" });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "1");

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "7efcac2e-3363-4cb1-8a68-5b0f30def307");

            migrationBuilder.RenameColumn(
                name: "totalsold",
                table: "Products",
                newName: "TotalSold");

            migrationBuilder.AddColumn<string>(
                name: "ClosingDay",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OpeningDay",
                table: "Shops",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClosingDay",
                table: "Shops");

            migrationBuilder.DropColumn(
                name: "OpeningDay",
                table: "Shops");

            migrationBuilder.RenameColumn(
                name: "TotalSold",
                table: "Products",
                newName: "totalsold");

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
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName", "ZipCode" },
                values: new object[] { "7efcac2e-3363-4cb1-8a68-5b0f30def307", 0, null, null, "dd5452cf-2e93-40db-8eb1-8fcb263a66db", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAECvFPhq6UqXqIaru/QhRcNszA583RQQi8/QGG92GMs9dp71daW7LxpxEMzYDQe4zvg==", null, false, null, "228233ba-f742-4681-b1c3-c27d75b58228", false, "admin@flexiro.com", null });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "7efcac2e-3363-4cb1-8a68-5b0f30def307" });
        }
    }
}
