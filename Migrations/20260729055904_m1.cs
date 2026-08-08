using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Smart_Farm_and_Crop_Yeild_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class m1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeclineNotes",
                table: "CropOrder",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclineReason",
                table: "CropOrder",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeclinedDate",
                table: "CropOrder",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalQuantity",
                table: "CropListing",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclineNotes",
                table: "CropOrder");

            migrationBuilder.DropColumn(
                name: "DeclineReason",
                table: "CropOrder");

            migrationBuilder.DropColumn(
                name: "DeclinedDate",
                table: "CropOrder");

            migrationBuilder.DropColumn(
                name: "OriginalQuantity",
                table: "CropListing");
        }
    }
}
