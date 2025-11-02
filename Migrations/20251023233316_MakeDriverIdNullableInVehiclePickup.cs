using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoEdge.Migrations
{
    /// <inheritdoc />
    public partial class MakeDriverIdNullableInVehiclePickup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehiclePickups_Drivers_DriverId",
                table: "VehiclePickups");

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "VehiclePickups",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_VehiclePickups_Drivers_DriverId",
                table: "VehiclePickups",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehiclePickups_Drivers_DriverId",
                table: "VehiclePickups");

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "VehiclePickups",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VehiclePickups_Drivers_DriverId",
                table: "VehiclePickups",
                column: "DriverId",
                principalTable: "Drivers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
