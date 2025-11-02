using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class AddRequirementsToDocumentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "DocumentTypes",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "DocumentTypes");
        }
    }
}
