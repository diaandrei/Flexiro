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
                keyValues: new object[] { "1", "bb23fa6a-6cb3-4f11-92e7-aee34f7fc334" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "bb23fa6a-6cb3-4f11-92e7-aee34f7fc334");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "f4f44f8d-8870-4384-97be-a162561d23fd", 0, null, null, "1400f11a-c90d-4d3d-b658-05940a196ba8", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAEDAZ11NmXsdJw6xWIhfXz7zsLEkO49z0KniCPxXZkH+s8JG27xNCNz4tz+nNn5dT6Q==", null, false, null, null, "9d282051-94f0-4a94-ad77-df8cf823326d", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "f4f44f8d-8870-4384-97be-a162561d23fd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "1", "f4f44f8d-8870-4384-97be-a162561d23fd" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "f4f44f8d-8870-4384-97be-a162561d23fd");

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "Address", "City", "ConcurrencyStamp", "Country", "CreatedAt", "Email", "EmailConfirmed", "FirstName", "IsAdmin", "IsSeller", "LastName", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "Postcode", "ProfilePic", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "bb23fa6a-6cb3-4f11-92e7-aee34f7fc334", 0, null, null, "d75c028c-87f9-40b5-bfb5-70c8901b026d", null, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@flexiro.com", true, "Admin", true, false, "Admin", false, null, "ADMIN@FLEXIRO.COM", "ADMIN@FLEXIRO.COM", "AQAAAAIAAYagAAAAELrsZkAKe0j5f6UdW4wkawEhkbNOnZRX2Bm1g+HKnGC+UmUpyViDtMaBnJp0rts3fw==", null, false, null, null, "79a1cc12-223e-41ed-bae2-1217306c882f", false, "admin@flexiro.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "1", "bb23fa6a-6cb3-4f11-92e7-aee34f7fc334" });
        }
    }
}
