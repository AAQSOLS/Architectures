using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ModularSaaS.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Identifier",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId_Name",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_PlatformUsers_Email",
                schema: "identity",
                table: "PlatformUsers");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                schema: "identity",
                table: "Users",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                schema: "identity",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "identity",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                schema: "tenancy",
                table: "Tenants",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                schema: "tenancy",
                table: "Tenants",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "tenancy",
                table: "Tenants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                schema: "identity",
                table: "Roles",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                schema: "identity",
                table: "Roles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "identity",
                table: "Roles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAtUtc",
                schema: "identity",
                table: "PlatformUsers",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedBy",
                schema: "identity",
                table: "PlatformUsers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "identity",
                table: "PlatformUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                schema: "identity",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Identifier",
                schema: "tenancy",
                table: "Tenants",
                column: "Identifier",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                schema: "identity",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PlatformUsers_Email",
                schema: "identity",
                table: "PlatformUsers",
                column: "Email",
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_TenantId_Email",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Identifier",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_Roles_TenantId_Name",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_PlatformUsers_Email",
                schema: "identity",
                table: "PlatformUsers");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "tenancy",
                table: "Tenants");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "identity",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "DeletedAtUtc",
                schema: "identity",
                table: "PlatformUsers");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "identity",
                table: "PlatformUsers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "identity",
                table: "PlatformUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Users_TenantId_Email",
                schema: "identity",
                table: "Users",
                columns: new[] { "TenantId", "Email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Identifier",
                schema: "tenancy",
                table: "Tenants",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_TenantId_Name",
                schema: "identity",
                table: "Roles",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlatformUsers_Email",
                schema: "identity",
                table: "PlatformUsers",
                column: "Email",
                unique: true);
        }
    }
}
