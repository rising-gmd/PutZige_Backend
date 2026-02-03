using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PutZige.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationTrackingToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationSentCount",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastEmailVerificationSentAt",
                table: "Users",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationSentCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastEmailVerificationSentAt",
                table: "Users");
        }
    }
}
