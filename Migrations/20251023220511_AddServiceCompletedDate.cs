using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddServiceCompletedDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ServiceCompletedDate",
                table: "ServiceBookings",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceCompletedDate",
                table: "ServiceBookings");
        }
    }
}
