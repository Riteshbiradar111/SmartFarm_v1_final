using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm_and_Crop_Yeild_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CropCycle_LandPlot_PlotId",
                table: "CropCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_CropListing_Harvest_HarvestId",
                table: "CropListing");

            migrationBuilder.DropForeignKey(
                name: "FK_CropMonitoring_CropCycle_CropCycleId",
                table: "CropMonitoring");

            migrationBuilder.DropForeignKey(
                name: "FK_Farm_Farmer_FarmerId",
                table: "Farm");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvest_CropCycle_CropCycleId",
                table: "Harvest");

            migrationBuilder.DropForeignKey(
                name: "FK_LandPlot_Farm_FarmId",
                table: "LandPlot");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Users_UserId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_PestCase_CropCycle_CropCycleId",
                table: "PestCase");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorReading_LandPlot_PlotId",
                table: "SensorReading");

            migrationBuilder.AddForeignKey(
                name: "FK_CropCycle_LandPlot_PlotId",
                table: "CropCycle",
                column: "PlotId",
                principalTable: "LandPlot",
                principalColumn: "PlotId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CropListing_Harvest_HarvestId",
                table: "CropListing",
                column: "HarvestId",
                principalTable: "Harvest",
                principalColumn: "HarvestId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CropMonitoring_CropCycle_CropCycleId",
                table: "CropMonitoring",
                column: "CropCycleId",
                principalTable: "CropCycle",
                principalColumn: "CropCycleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Farm_Farmer_FarmerId",
                table: "Farm",
                column: "FarmerId",
                principalTable: "Farmer",
                principalColumn: "FarmerId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvest_CropCycle_CropCycleId",
                table: "Harvest",
                column: "CropCycleId",
                principalTable: "CropCycle",
                principalColumn: "CropCycleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LandPlot_Farm_FarmId",
                table: "LandPlot",
                column: "FarmId",
                principalTable: "Farm",
                principalColumn: "FarmId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Users_UserId",
                table: "Notification",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PestCase_CropCycle_CropCycleId",
                table: "PestCase",
                column: "CropCycleId",
                principalTable: "CropCycle",
                principalColumn: "CropCycleId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorReading_LandPlot_PlotId",
                table: "SensorReading",
                column: "PlotId",
                principalTable: "LandPlot",
                principalColumn: "PlotId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CropCycle_LandPlot_PlotId",
                table: "CropCycle");

            migrationBuilder.DropForeignKey(
                name: "FK_CropListing_Harvest_HarvestId",
                table: "CropListing");

            migrationBuilder.DropForeignKey(
                name: "FK_CropMonitoring_CropCycle_CropCycleId",
                table: "CropMonitoring");

            migrationBuilder.DropForeignKey(
                name: "FK_Farm_Farmer_FarmerId",
                table: "Farm");

            migrationBuilder.DropForeignKey(
                name: "FK_Harvest_CropCycle_CropCycleId",
                table: "Harvest");

            migrationBuilder.DropForeignKey(
                name: "FK_LandPlot_Farm_FarmId",
                table: "LandPlot");

            migrationBuilder.DropForeignKey(
                name: "FK_Notification_Users_UserId",
                table: "Notification");

            migrationBuilder.DropForeignKey(
                name: "FK_PestCase_CropCycle_CropCycleId",
                table: "PestCase");

            migrationBuilder.DropForeignKey(
                name: "FK_SensorReading_LandPlot_PlotId",
                table: "SensorReading");

            migrationBuilder.AddForeignKey(
                name: "FK_CropCycle_LandPlot_PlotId",
                table: "CropCycle",
                column: "PlotId",
                principalTable: "LandPlot",
                principalColumn: "PlotId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CropListing_Harvest_HarvestId",
                table: "CropListing",
                column: "HarvestId",
                principalTable: "Harvest",
                principalColumn: "HarvestId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CropMonitoring_CropCycle_CropCycleId",
                table: "CropMonitoring",
                column: "CropCycleId",
                principalTable: "CropCycle",
                principalColumn: "CropCycleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Farm_Farmer_FarmerId",
                table: "Farm",
                column: "FarmerId",
                principalTable: "Farmer",
                principalColumn: "FarmerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Harvest_CropCycle_CropCycleId",
                table: "Harvest",
                column: "CropCycleId",
                principalTable: "CropCycle",
                principalColumn: "CropCycleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LandPlot_Farm_FarmId",
                table: "LandPlot",
                column: "FarmId",
                principalTable: "Farm",
                principalColumn: "FarmId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Notification_Users_UserId",
                table: "Notification",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PestCase_CropCycle_CropCycleId",
                table: "PestCase",
                column: "CropCycleId",
                principalTable: "CropCycle",
                principalColumn: "CropCycleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SensorReading_LandPlot_PlotId",
                table: "SensorReading",
                column: "PlotId",
                principalTable: "LandPlot",
                principalColumn: "PlotId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
