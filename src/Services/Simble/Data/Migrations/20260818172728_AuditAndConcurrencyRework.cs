using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Simble.Data.Migrations
{
    /// <inheritdoc />
    public partial class AuditAndConcurrencyRework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_samples_Name",
                table: "samples");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "samples");

            migrationBuilder.RenameColumn(
                name: "LastUpdateBy",
                table: "samples",
                newName: "LastUpdatedBy");

            migrationBuilder.RenameColumn(
                name: "LastUpdateAt",
                table: "samples",
                newName: "LastUpdatedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatedBy",
                table: "samples",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Version",
                table: "samples",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.CreateIndex(
                name: "IX_samples_IsDeleted",
                table: "samples",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_samples_Name",
                table: "samples",
                column: "Name",
                unique: true,
                filter: "\"IsDeleted\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_samples_IsDeleted",
                table: "samples");

            migrationBuilder.DropIndex(
                name: "IX_samples_Name",
                table: "samples");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "samples");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "samples");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedBy",
                table: "samples",
                newName: "LastUpdateBy");

            migrationBuilder.RenameColumn(
                name: "LastUpdatedAt",
                table: "samples",
                newName: "LastUpdateAt");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "samples",
                type: "bytea",
                rowVersion: true,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_samples_Name",
                table: "samples",
                column: "Name",
                unique: true);
        }
    }
}
