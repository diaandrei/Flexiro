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
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "f4f44f8d-8870-4384-97be-a162561d23fd" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f4f44f8d-8870-4384-97be-a162561d23fd");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Carts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "35022332-7280-4cb8-af66-b71e95085b32", 0, null, null, "b8110a44-5afe-4c45-941b-45e5bc1613f2", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAEDML0m3pZDYw5/Ss/yUZb5dq+vAMUQ48ECT7EPst+03h6d/jeyN2zSU6Gxo0q0nVSg==", null, false, null, null, "e11cf133-a56b-4db7-b440-a1f9415e4c2d", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "35022332-7280-4cb8-af66-b71e95085b32" });

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts");

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "35022332-7280-4cb8-af66-b71e95085b32" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "35022332-7280-4cb8-af66-b71e95085b32");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Carts",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "f4f44f8d-8870-4384-97be-a162561d23fd", 0, null, null, "1400f11a-c90d-4d3d-b658-05940a196ba8", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAEDAZ11NmXsdJw6xWIhfXz7zsLEkO49z0KniCPxXZkH+s8JG27xNCNz4tz+nNn5dT6Q==", null, false, null, null, "9d282051-94f0-4a94-ad77-df8cf823326d", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "f4f44f8d-8870-4384-97be-a162561d23fd" });

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_UserId",
                table: "Carts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
