using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddSigningRequestIdToContract : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SigningRequestId",
                table: "Contracts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SigningRequestId",
                table: "Contracts");
        }
    }
}
