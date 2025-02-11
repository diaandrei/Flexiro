using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Flexiro.Application.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "35022332-7280-4cb8-af66-b71e95085b32" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "35022332-7280-4cb8-af66-b71e95085b32");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "5170cc74-9ec5-441b-9d2a-2a0f47e5c416", 0, null, null, "44d54bc6-3a56-4191-93f4-d86904157ec6", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAEOazUYHI95lEsu+u2m4DUp80glc8/m2MfJFRiWxh/xu0apxiSQNpugHz5du14fnEPw==", null, false, null, null, "a5f2c1f6-ea8f-4b1d-9b7f-b415cf43c5dd", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "5170cc74-9ec5-441b-9d2a-2a0f47e5c416" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "5170cc74-9ec5-441b-9d2a-2a0f47e5c416" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "5170cc74-9ec5-441b-9d2a-2a0f47e5c416");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "35022332-7280-4cb8-af66-b71e95085b32", 0, null, null, "b8110a44-5afe-4c45-941b-45e5bc1613f2", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAEDML0m3pZDYw5/Ss/yUZb5dq+vAMUQ48ECT7EPst+03h6d/jeyN2zSU6Gxo0q0nVSg==", null, false, null, null, "e11cf133-a56b-4db7-b440-a1f9415e4c2d", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "35022332-7280-4cb8-af66-b71e95085b32" });
        }
    }
}
