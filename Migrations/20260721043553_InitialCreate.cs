using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Smart_Farm_and_Crop_Yeild_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Crop",
                columns: table => new
                {
                    CropId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CropName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Season = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Crop", x => x.CropId);
                });

            migrationBuilder.CreateTable(
                name: "Report",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RelatedModule = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RelatedEntityId = table.Column<int>(type: "int", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExportedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExported = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Report", x => x.ReportId);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsBlocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    LastLogin = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdminProfile",
                columns: table => new
                {
                    ProfileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmployeeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminProfile", x => x.ProfileId);
                    table.ForeignKey(
                        name: "FK_AdminProfile_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Agronomist",
                columns: table => new
                {
                    AgronomistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Specialization = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agronomist", x => x.AgronomistId);
                    table.ForeignKey(
                        name: "FK_Agronomist_Users",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Buyer",
                columns: table => new
                {
                    BuyerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CompanyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    BusinessAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyer", x => x.BuyerId);
                    table.ForeignKey(
                        name: "FK__Buyer__UserId__44FF419A",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CooperativeManager",
                columns: table => new
                {
                    ManagerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CooperativeName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Region = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CooperativeManager", x => x.ManagerId);
                    table.ForeignKey(
                        name: "FK_CooperativeManager_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Farmer",
                columns: table => new
                {
                    FarmerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Village = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PinCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Taluka = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyContact = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farmer", x => x.FarmerId);
                    table.ForeignKey(
                        name: "FK__Farmer__UserId__403A8C7D",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldOfficer",
                columns: table => new
                {
                    OfficerId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    AssignedDistrict = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AssignedTaluka = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldOfficer", x => x.OfficerId);
                    table.ForeignKey(
                        name: "FK_FieldOfficer_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notification", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notification_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Farm",
                columns: table => new
                {
                    FarmId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    FarmName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Village = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Taluka = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    District = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Pincode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farm", x => x.FarmId);
                    table.ForeignKey(
                        name: "FK_Farm_Farmer_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FieldOfficerAssignment",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FieldOfficerUserId = table.Column<int>(type: "int", nullable: false),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldOfficerAssignment", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_FOA_Farmer",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId");
                    table.ForeignKey(
                        name: "FK_FOA_Users",
                        column: x => x.FieldOfficerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Wishlist",
                columns: table => new
                {
                    WishlistId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    CropId = table.Column<int>(type: "int", nullable: true),
                    FarmerId = table.Column<int>(type: "int", nullable: true),
                    NotifyWhenAvailable = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wishlist", x => x.WishlistId);
                    table.ForeignKey(
                        name: "FK_Wishlist_Buyer_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyer",
                        principalColumn: "BuyerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Wishlist_Crop_CropId",
                        column: x => x.CropId,
                        principalTable: "Crop",
                        principalColumn: "CropId");
                    table.ForeignKey(
                        name: "FK_Wishlist_Farmer_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId");
                });

            migrationBuilder.CreateTable(
                name: "Assignment",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    OfficerId = table.Column<int>(type: "int", nullable: false),
                    Task = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assignment", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_Assignment_Farm_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farm",
                        principalColumn: "FarmId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assignment_Farmer_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Assignment_Users_OfficerId",
                        column: x => x.OfficerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LandPlot",
                columns: table => new
                {
                    PlotId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    PlotName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlotCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Area = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AreaUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Latitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    Longitude = table.Column<decimal>(type: "decimal(9,6)", precision: 9, scale: 6, nullable: false),
                    SoilType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IrrigationType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LandPlot", x => x.PlotId);
                    table.ForeignKey(
                        name: "FK_LandPlot_Farm_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farm",
                        principalColumn: "FarmId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CropCycle",
                columns: table => new
                {
                    CropCycleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlotId = table.Column<int>(type: "int", nullable: false),
                    CropId = table.Column<int>(type: "int", nullable: false),
                    SowingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedHarvestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CurrentStage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropCycle", x => x.CropCycleId);
                    table.ForeignKey(
                        name: "FK_CropCycle_Crop_CropId",
                        column: x => x.CropId,
                        principalTable: "Crop",
                        principalColumn: "CropId");
                    table.ForeignKey(
                        name: "FK_CropCycle_LandPlot_PlotId",
                        column: x => x.PlotId,
                        principalTable: "LandPlot",
                        principalColumn: "PlotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CultivationRequest",
                columns: table => new
                {
                    RequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    PlotId = table.Column<int>(type: "int", nullable: false),
                    CropId = table.Column<int>(type: "int", nullable: false),
                    CultivationArea = table.Column<decimal>(type: "decimal(10,2)", precision: 18, scale: 2, nullable: false),
                    SowingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoilPH = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    MoistureLevel = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false, defaultValue: "Pending"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CultivationRequest", x => x.RequestId);
                    table.ForeignKey(
                        name: "FK_CultivationRequest_Crop",
                        column: x => x.CropId,
                        principalTable: "Crop",
                        principalColumn: "CropId");
                    table.ForeignKey(
                        name: "FK_CultivationRequest_Farm",
                        column: x => x.FarmId,
                        principalTable: "Farm",
                        principalColumn: "FarmId");
                    table.ForeignKey(
                        name: "FK_CultivationRequest_Farmer",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId");
                    table.ForeignKey(
                        name: "FK_CultivationRequest_LandPlot",
                        column: x => x.PlotId,
                        principalTable: "LandPlot",
                        principalColumn: "PlotId");
                });

            migrationBuilder.CreateTable(
                name: "FieldVisit",
                columns: table => new
                {
                    VisitId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    PlotId = table.Column<int>(type: "int", nullable: true),
                    AssignedOfficerId = table.Column<int>(type: "int", nullable: false),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VisitTime = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldVisit", x => x.VisitId);
                    table.ForeignKey(
                        name: "FK_FieldVisit_Farmer",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId");
                    table.ForeignKey(
                        name: "FK_FieldVisit_LandPlot",
                        column: x => x.PlotId,
                        principalTable: "LandPlot",
                        principalColumn: "PlotId");
                    table.ForeignKey(
                        name: "FK_FieldVisit_Officer",
                        column: x => x.AssignedOfficerId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "SensorReading",
                columns: table => new
                {
                    ReadingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlotId = table.Column<int>(type: "int", nullable: false),
                    SoilMoisture = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SoilPH = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    Nitrogen = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Phosphorus = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Potassium = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    ElectricalConductivity = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    OrganicCarbon = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReading", x => x.ReadingId);
                    table.ForeignKey(
                        name: "FK_SensorReading_LandPlot_PlotId",
                        column: x => x.PlotId,
                        principalTable: "LandPlot",
                        principalColumn: "PlotId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SupportQuery",
                columns: table => new
                {
                    QueryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    QueryType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FarmId = table.Column<int>(type: "int", nullable: false),
                    PlotId = table.Column<int>(type: "int", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedToUserId = table.Column<int>(type: "int", nullable: true),
                    ResolutionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AgronomistRecommendation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RecommendationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VisitDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OfficerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FieldObservation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ActionTaken = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ReportImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ImprovementActions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ImprovementExpectedBenefits = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImprovementStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupportQuery", x => x.QueryId);
                    table.ForeignKey(
                        name: "FK_SupportQuery_Farm_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farm",
                        principalColumn: "FarmId");
                    table.ForeignKey(
                        name: "FK_SupportQuery_Farmer_FarmerId",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId");
                    table.ForeignKey(
                        name: "FK_SupportQuery_LandPlot_PlotId",
                        column: x => x.PlotId,
                        principalTable: "LandPlot",
                        principalColumn: "PlotId");
                    table.ForeignKey(
                        name: "FK_SupportQuery_Users_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "YieldRecord",
                columns: table => new
                {
                    YieldId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    PlotId = table.Column<int>(type: "int", nullable: true),
                    CropId = table.Column<int>(type: "int", nullable: true),
                    SubmittedByUserId = table.Column<int>(type: "int", nullable: true),
                    Area = table.Column<decimal>(type: "decimal(12,4)", precision: 12, scale: 4, nullable: true),
                    EstimatedYield = table.Column<decimal>(type: "decimal(14,2)", precision: 14, scale: 2, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldRecord", x => x.YieldId);
                    table.ForeignKey(
                        name: "FK_YieldRecord_Crop",
                        column: x => x.CropId,
                        principalTable: "Crop",
                        principalColumn: "CropId");
                    table.ForeignKey(
                        name: "FK_YieldRecord_Farmer",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId");
                    table.ForeignKey(
                        name: "FK_YieldRecord_LandPlot",
                        column: x => x.PlotId,
                        principalTable: "LandPlot",
                        principalColumn: "PlotId");
                    table.ForeignKey(
                        name: "FK_YieldRecord_Users",
                        column: x => x.SubmittedByUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CropMonitoring",
                columns: table => new
                {
                    MonitoringId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CropCycleId = table.Column<int>(type: "int", nullable: false),
                    ObservationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GrowthStage = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PlantHeight = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CropHealth = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropMonitoring", x => x.MonitoringId);
                    table.ForeignKey(
                        name: "FK_CropMonitoring_CropCycle_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycle",
                        principalColumn: "CropCycleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Harvest",
                columns: table => new
                {
                    HarvestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CropCycleId = table.Column<int>(type: "int", nullable: false),
                    HarvestDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ActualQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Harvest", x => x.HarvestId);
                    table.ForeignKey(
                        name: "FK_Harvest_CropCycle_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycle",
                        principalColumn: "CropCycleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PestCase",
                columns: table => new
                {
                    PestCaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CropCycleId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    ReportUploadedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FieldVisitCompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FarmerResponseToReport = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FarmerResponseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    ClosedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FieldVisitRequested = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssignedOfficerId = table.Column<int>(type: "int", nullable: true),
                    FieldReport = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Recommendation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PestCase", x => x.PestCaseId);
                    table.ForeignKey(
                        name: "FK_PestCase_CropCycle_CropCycleId",
                        column: x => x.CropCycleId,
                        principalTable: "CropCycle",
                        principalColumn: "CropCycleId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_PestCase_Users_AssignedOfficerId",
                        column: x => x.AssignedOfficerId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AgronomistAnalysis",
                columns: table => new
                {
                    AnalysisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RequestId = table.Column<int>(type: "int", nullable: false),
                    AgronomistId = table.Column<int>(type: "int", nullable: false),
                    SoilAnalysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    WeatherAnalysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CropAnalysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PestAnalysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DiseaseAnalysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgronomistAnalysis", x => x.AnalysisId);
                    table.ForeignKey(
                        name: "FK_AgronomistAnalysis_Agronomist",
                        column: x => x.AgronomistId,
                        principalTable: "Agronomist",
                        principalColumn: "AgronomistId");
                    table.ForeignKey(
                        name: "FK_AgronomistAnalysis_Request",
                        column: x => x.RequestId,
                        principalTable: "CultivationRequest",
                        principalColumn: "RequestId");
                });

            migrationBuilder.CreateTable(
                name: "VisitPhoto",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VisitId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPhoto", x => x.PhotoId);
                    table.ForeignKey(
                        name: "FK_VisitPhoto_FieldVisit",
                        column: x => x.VisitId,
                        principalTable: "FieldVisit",
                        principalColumn: "VisitId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YieldPhoto",
                columns: table => new
                {
                    PhotoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YieldId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldPhoto", x => x.PhotoId);
                    table.ForeignKey(
                        name: "FK_YieldPhoto_YieldRecord",
                        column: x => x.YieldId,
                        principalTable: "YieldRecord",
                        principalColumn: "YieldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YieldValidation",
                columns: table => new
                {
                    ValidationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    YieldId = table.Column<int>(type: "int", nullable: false),
                    FieldOfficerUserId = table.Column<int>(type: "int", nullable: false),
                    ValidationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    ValidationStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YieldValidation", x => x.ValidationId);
                    table.ForeignKey(
                        name: "FK_YieldValidation_Users",
                        column: x => x.FieldOfficerUserId,
                        principalTable: "Users",
                        principalColumn: "UserId");
                    table.ForeignKey(
                        name: "FK_YieldValidation_YieldRecord",
                        column: x => x.YieldId,
                        principalTable: "YieldRecord",
                        principalColumn: "YieldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CropListing",
                columns: table => new
                {
                    ListingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HarvestId = table.Column<int>(type: "int", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AvailableQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ListedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())"),
                    ImagePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BuyerId = table.Column<int>(type: "int", nullable: true),
                    PurchasedQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropListing", x => x.ListingId);
                    table.ForeignKey(
                        name: "FK_CropListing_Buyer_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyer",
                        principalColumn: "BuyerId");
                    table.ForeignKey(
                        name: "FK_CropListing_Harvest_HarvestId",
                        column: x => x.HarvestId,
                        principalTable: "Harvest",
                        principalColumn: "HarvestId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HarvestDecision",
                columns: table => new
                {
                    DecisionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HarvestId = table.Column<int>(type: "int", nullable: false),
                    AgronomistId = table.Column<int>(type: "int", nullable: false),
                    CropHealthAnalysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    PestAnalysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    DiseaseAnalysis = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Decision = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HarvestDecision", x => x.DecisionId);
                    table.ForeignKey(
                        name: "FK_HarvestDecision_Agronomist",
                        column: x => x.AgronomistId,
                        principalTable: "Agronomist",
                        principalColumn: "AgronomistId");
                    table.ForeignKey(
                        name: "FK_HarvestDecision_Harvest",
                        column: x => x.HarvestId,
                        principalTable: "Harvest",
                        principalColumn: "HarvestId");
                });

            migrationBuilder.CreateTable(
                name: "CropOrder",
                columns: table => new
                {
                    OrderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ListingId = table.Column<int>(type: "int", nullable: true),
                    HarvestId = table.Column<int>(type: "int", nullable: true),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    FarmerId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AcceptedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GST = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DeliveryAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SpecialInstructions = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CropOrder", x => x.OrderId);
                    table.ForeignKey(
                        name: "FK_CropOrder_Buyer",
                        column: x => x.BuyerId,
                        principalTable: "Buyer",
                        principalColumn: "BuyerId");
                    table.ForeignKey(
                        name: "FK_CropOrder_CropListing",
                        column: x => x.ListingId,
                        principalTable: "CropListing",
                        principalColumn: "ListingId");
                    table.ForeignKey(
                        name: "FK_CropOrder_Farmer",
                        column: x => x.FarmerId,
                        principalTable: "Farmer",
                        principalColumn: "FarmerId");
                    table.ForeignKey(
                        name: "FK_CropOrder_Harvest",
                        column: x => x.HarvestId,
                        principalTable: "Harvest",
                        principalColumn: "HarvestId");
                });

            migrationBuilder.CreateTable(
                name: "BuyerComplaint",
                columns: table => new
                {
                    ComplaintId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuyerId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    ComplaintType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResolutionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ResolvedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerComplaint", x => x.ComplaintId);
                    table.ForeignKey(
                        name: "FK_BuyerComplaint_Buyer_BuyerId",
                        column: x => x.BuyerId,
                        principalTable: "Buyer",
                        principalColumn: "BuyerId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BuyerComplaint_CropOrder_OrderId",
                        column: x => x.OrderId,
                        principalTable: "CropOrder",
                        principalColumn: "OrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Farmer" },
                    { 3, "Buyer" },
                    { 4, "Agronomist" },
                    { 5, "Field Officer" },
                    { 6, "Cooperative Manager" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "CreatedAt", "Email", "FullName", "IsActive", "LastLogin", "PasswordHash", "Phone", "RoleId", "Username" },
                values: new object[] { 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "admin@smartfarm.com", null, true, null, "admin123", null, 1, "admin" });

            migrationBuilder.InsertData(
                table: "AdminProfile",
                columns: new[] { "ProfileId", "Address", "City", "Department", "EmployeeId", "FirstName", "LastName", "PinCode", "State", "UpdatedAt", "UserId" },
                values: new object[] { 1, null, null, "IT", "ADMIN001", "System", "Administrator", null, null, null, 1 });

            migrationBuilder.CreateIndex(
                name: "IX_AdminProfile_UserId",
                table: "AdminProfile",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "UQ_Agronomist_MobileNumber",
                table: "Agronomist",
                column: "MobileNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Agronomist_UserId",
                table: "Agronomist",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgronomistAnalysis_AgronomistId",
                table: "AgronomistAnalysis",
                column: "AgronomistId");

            migrationBuilder.CreateIndex(
                name: "IX_AgronomistAnalysis_RequestId",
                table: "AgronomistAnalysis",
                column: "RequestId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_FarmerId",
                table: "Assignment",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_FarmId",
                table: "Assignment",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Assignment_OfficerId",
                table: "Assignment",
                column: "OfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_Buyer_UserId",
                table: "Buyer",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerComplaint_BuyerId",
                table: "BuyerComplaint",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerComplaint_OrderId",
                table: "BuyerComplaint",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CooperativeManager_UserId",
                table: "CooperativeManager",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CropCycle_CropId",
                table: "CropCycle",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_CropCycle_PlotId",
                table: "CropCycle",
                column: "PlotId");

            migrationBuilder.CreateIndex(
                name: "IX_CropListing_BuyerId",
                table: "CropListing",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_CropListing_HarvestId",
                table: "CropListing",
                column: "HarvestId");

            migrationBuilder.CreateIndex(
                name: "IX_CropMonitoring_CropCycleId",
                table: "CropMonitoring",
                column: "CropCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_CropOrder_BuyerId",
                table: "CropOrder",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_CropOrder_FarmerId",
                table: "CropOrder",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_CropOrder_HarvestId",
                table: "CropOrder",
                column: "HarvestId");

            migrationBuilder.CreateIndex(
                name: "IX_CropOrder_ListingId",
                table: "CropOrder",
                column: "ListingId");

            migrationBuilder.CreateIndex(
                name: "IX_CultivationRequest_CropId",
                table: "CultivationRequest",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_CultivationRequest_FarmerId",
                table: "CultivationRequest",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_CultivationRequest_FarmId",
                table: "CultivationRequest",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_CultivationRequest_PlotId",
                table: "CultivationRequest",
                column: "PlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Farm_FarmerId",
                table: "Farm",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_Farmer_UserId",
                table: "Farmer",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldOfficer_UserId",
                table: "FieldOfficer",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldOfficerAssignment_FarmerId",
                table: "FieldOfficerAssignment",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldOfficerAssignment_FieldOfficerUserId",
                table: "FieldOfficerAssignment",
                column: "FieldOfficerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldVisit_AssignedOfficerId",
                table: "FieldVisit",
                column: "AssignedOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldVisit_FarmerId",
                table: "FieldVisit",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_FieldVisit_PlotId",
                table: "FieldVisit",
                column: "PlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Harvest_CropCycleId",
                table: "Harvest",
                column: "CropCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestDecision_AgronomistId",
                table: "HarvestDecision",
                column: "AgronomistId");

            migrationBuilder.CreateIndex(
                name: "IX_HarvestDecision_HarvestId",
                table: "HarvestDecision",
                column: "HarvestId");

            migrationBuilder.CreateIndex(
                name: "IX_LandPlot_FarmId",
                table: "LandPlot",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PestCase_AssignedOfficerId",
                table: "PestCase",
                column: "AssignedOfficerId");

            migrationBuilder.CreateIndex(
                name: "IX_PestCase_CropCycleId",
                table: "PestCase",
                column: "CropCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_SensorReading_PlotId",
                table: "SensorReading",
                column: "PlotId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportQuery_AssignedToUserId",
                table: "SupportQuery",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportQuery_FarmerId",
                table: "SupportQuery",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportQuery_FarmId",
                table: "SupportQuery",
                column: "FarmId");

            migrationBuilder.CreateIndex(
                name: "IX_SupportQuery_PlotId",
                table: "SupportQuery",
                column: "PlotId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_VisitPhoto_VisitId",
                table: "VisitPhoto",
                column: "VisitId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_BuyerId",
                table: "Wishlist",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_CropId",
                table: "Wishlist",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_Wishlist_FarmerId",
                table: "Wishlist",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldPhoto_YieldId",
                table: "YieldPhoto",
                column: "YieldId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldRecord_CropId",
                table: "YieldRecord",
                column: "CropId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldRecord_FarmerId",
                table: "YieldRecord",
                column: "FarmerId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldRecord_PlotId",
                table: "YieldRecord",
                column: "PlotId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldRecord_SubmittedByUserId",
                table: "YieldRecord",
                column: "SubmittedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldValidation_FieldOfficerUserId",
                table: "YieldValidation",
                column: "FieldOfficerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_YieldValidation_YieldId",
                table: "YieldValidation",
                column: "YieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminProfile");

            migrationBuilder.DropTable(
                name: "AgronomistAnalysis");

            migrationBuilder.DropTable(
                name: "Assignment");

            migrationBuilder.DropTable(
                name: "BuyerComplaint");

            migrationBuilder.DropTable(
                name: "CooperativeManager");

            migrationBuilder.DropTable(
                name: "CropMonitoring");

            migrationBuilder.DropTable(
                name: "FieldOfficer");

            migrationBuilder.DropTable(
                name: "FieldOfficerAssignment");

            migrationBuilder.DropTable(
                name: "HarvestDecision");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "PestCase");

            migrationBuilder.DropTable(
                name: "Report");

            migrationBuilder.DropTable(
                name: "SensorReading");

            migrationBuilder.DropTable(
                name: "SupportQuery");

            migrationBuilder.DropTable(
                name: "VisitPhoto");

            migrationBuilder.DropTable(
                name: "Wishlist");

            migrationBuilder.DropTable(
                name: "YieldPhoto");

            migrationBuilder.DropTable(
                name: "YieldValidation");

            migrationBuilder.DropTable(
                name: "CultivationRequest");

            migrationBuilder.DropTable(
                name: "CropOrder");

            migrationBuilder.DropTable(
                name: "Agronomist");

            migrationBuilder.DropTable(
                name: "FieldVisit");

            migrationBuilder.DropTable(
                name: "YieldRecord");

            migrationBuilder.DropTable(
                name: "CropListing");

            migrationBuilder.DropTable(
                name: "Buyer");

            migrationBuilder.DropTable(
                name: "Harvest");

            migrationBuilder.DropTable(
                name: "CropCycle");

            migrationBuilder.DropTable(
                name: "Crop");

            migrationBuilder.DropTable(
                name: "LandPlot");

            migrationBuilder.DropTable(
                name: "Farm");

            migrationBuilder.DropTable(
                name: "Farmer");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
