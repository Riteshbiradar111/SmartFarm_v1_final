using Microsoft.EntityFrameworkCore;

namespace Smart_Farm_and_Crop_Yeild_Management_System.Models
{
    public class SmartFarmDbContext : DbContext
    {
        public SmartFarmDbContext()
        {
        }

        public SmartFarmDbContext(DbContextOptions<SmartFarmDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Farmer> Farmers { get; set; } = null!;
        public virtual DbSet<Buyer> Buyers { get; set; } = null!;
        public virtual DbSet<Agronomist> Agronomists { get; set; } = null!;
        public virtual DbSet<Farm> Farms { get; set; } = null!;
        public virtual DbSet<LandPlot> LandPlots { get; set; } = null!;
        public virtual DbSet<Crop> Crops { get; set; } = null!;
        public virtual DbSet<CropCycle> CropCycles { get; set; } = null!;
        public virtual DbSet<CropMonitoring> CropMonitorings { get; set; } = null!;
        public virtual DbSet<PestCase> PestCases { get; set; } = null!;
        public virtual DbSet<Harvest> Harvests { get; set; } = null!;
        public virtual DbSet<CropListing> CropListings { get; set; } = null!;
        public virtual DbSet<CropOrder> CropOrders { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<SensorReading> SensorReadings { get; set; } = null!;
        public virtual DbSet<SupportQuery> SupportQueries { get; set; } = null!;
        public virtual DbSet<AdminProfile> AdminProfiles { get; set; } = null!;
        public virtual DbSet<FieldOfficer> FieldOfficers { get; set; } = null!;
        public virtual DbSet<FieldOfficerAssignment> FieldOfficerAssignments { get; set; } = null!;
        public virtual DbSet<CooperativeManager> CooperativeManagers { get; set; } = null!;
        public virtual DbSet<Assignment> Assignments { get; set; } = null!;
        public virtual DbSet<Report> Reports { get; set; } = null!;
        public virtual DbSet<FieldVisit> FieldVisits { get; set; } = null!;
        public virtual DbSet<VisitPhoto> VisitPhotos { get; set; } = null!;
        public virtual DbSet<YieldRecord> YieldRecords { get; set; } = null!;
        public virtual DbSet<YieldPhoto> YieldPhotos { get; set; } = null!;
        public virtual DbSet<Wishlist> Wishlists { get; set; } = null!;
        public virtual DbSet<BuyerComplaint> BuyerComplaints { get; set; } = null!;
        public virtual DbSet<CultivationRequest> CultivationRequests { get; set; } = null!;
        public virtual DbSet<AgronomistAnalysis> AgronomistAnalyses { get; set; } = null!;
        public virtual DbSet<HarvestDecision> HarvestDecisions { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Existing Roles mapping
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.RoleId);
                entity.Property(e => e.RoleName).HasMaxLength(50);
            });

            // Existing Users mapping
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.UserId);
                entity.Property(e => e.Username).HasMaxLength(50).IsRequired();
                entity.Property(e => e.PasswordHash).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(150).IsRequired();
                entity.Property(e => e.IsActive).HasDefaultValue(true).IsRequired();
                entity.Property(e => e.IsDeleted).HasDefaultValue(false).IsRequired();
                entity.Property(e => e.IsBlocked).HasDefaultValue(false).IsRequired();
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())").IsRequired();


                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Existing Farmer mapping
            modelBuilder.Entity<Farmer>(entity =>
            {
                entity.HasKey(e => e.FarmerId);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.MobileNumber).HasMaxLength(15);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.Village).HasMaxLength(100);
                entity.Property(e => e.District).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PinCode).HasMaxLength(10);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Farmers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Farmer__UserId__403A8C7D");
            });

            // Existing Buyer mapping
            modelBuilder.Entity<Buyer>(entity =>
            {
                entity.HasKey(e => e.BuyerId);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.CompanyName).HasMaxLength(150);
                entity.Property(e => e.MobileNumber).HasMaxLength(15);
                entity.Property(e => e.BusinessAddress).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.District).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PinCode).HasMaxLength(10);

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Buyers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK__Buyer__UserId__44FF419A");
            });

            // Agronomist mapping
            modelBuilder.Entity<Agronomist>(entity =>
            {
                entity.HasKey(e => e.AgronomistId);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.MobileNumber).HasMaxLength(15);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // New Farm mapping
            modelBuilder.Entity<Farm>(entity =>
            {
                entity.HasKey(e => e.FarmId);
                entity.Property(e => e.FarmName).HasMaxLength(150);
                entity.Property(e => e.Village).HasMaxLength(100);
                entity.Property(e => e.Taluka).HasMaxLength(100);
                entity.Property(e => e.District).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.Pincode).HasMaxLength(10);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Farmer)
                    .WithMany(p => p.Farms)
                    .HasForeignKey(d => d.FarmerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // New LandPlot mapping
            modelBuilder.Entity<LandPlot>(entity =>
            {
                entity.HasKey(e => e.PlotId);
                entity.Property(e => e.PlotName).HasMaxLength(100);
                entity.Property(e => e.PlotCode).HasMaxLength(50);
                entity.Property(e => e.Area).HasPrecision(18, 2);
                entity.Property(e => e.AreaUnit).HasMaxLength(50);
                entity.Property(e => e.Latitude).HasPrecision(9, 6);
                entity.Property(e => e.Longitude).HasPrecision(9, 6);
                entity.Property(e => e.SoilType).HasMaxLength(100);
                entity.Property(e => e.IrrigationType).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Farm)
                    .WithMany(p => p.LandPlots)
                    .HasForeignKey(d => d.FarmId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // New Crop mapping
            modelBuilder.Entity<Crop>(entity =>
            {
                entity.HasKey(e => e.CropId);
                entity.Property(e => e.CropName).HasMaxLength(100);
                entity.Property(e => e.Season).HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // New CropCycle mapping
            modelBuilder.Entity<CropCycle>(entity =>
            {
                entity.HasKey(e => e.CropCycleId);
                entity.Property(e => e.CurrentStage).HasMaxLength(100);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.LandPlot)
                    .WithMany(p => p.CropCycles)
                    .HasForeignKey(d => d.PlotId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Crop)
                    .WithMany(p => p.CropCycles)
                    .HasForeignKey(d => d.CropId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            // New CropMonitoring mapping
            modelBuilder.Entity<CropMonitoring>(entity =>
            {
                entity.HasKey(e => e.MonitoringId);
                entity.Property(e => e.GrowthStage).HasMaxLength(100);
                entity.Property(e => e.PlantHeight).HasPrecision(18, 2);
                entity.Property(e => e.CropHealth).HasMaxLength(100);
                entity.Property(e => e.Remarks).HasMaxLength(500);
                entity.Property(e => e.ImagePath).HasMaxLength(300);

                entity.HasOne(d => d.CropCycle)
                    .WithMany(p => p.CropMonitorings)
                    .HasForeignKey(d => d.CropCycleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // New PestCase mapping
            modelBuilder.Entity<PestCase>(entity =>
            {
                entity.HasKey(e => e.PestCaseId);
                entity.Property(e => e.Title).HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.ImagePath).HasMaxLength(300);
                entity.Property(e => e.Priority).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.FieldVisitRequested).HasDefaultValue(false);
                entity.Property(e => e.FieldReport).HasMaxLength(1000);
                entity.Property(e => e.Recommendation).HasMaxLength(1000);

                entity.HasOne(d => d.CropCycle)
                    .WithMany(p => p.PestCases)
                    .HasForeignKey(d => d.CropCycleId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.AssignedOfficer)
                    .WithMany()
                    .HasForeignKey(d => d.AssignedOfficerId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // New Harvest mapping
            modelBuilder.Entity<Harvest>(entity =>
            {
                entity.HasKey(e => e.HarvestId);
                entity.Property(e => e.ExpectedQuantity).HasPrecision(18, 2);
                entity.Property(e => e.ActualQuantity).HasPrecision(18, 2);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.CropCycle)
                    .WithMany(p => p.Harvests)
                    .HasForeignKey(d => d.CropCycleId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // New CropListing mapping
            modelBuilder.Entity<CropListing>(entity =>
            {
                entity.HasKey(e => e.ListingId);
                entity.Property(e => e.PricePerUnit).HasPrecision(18, 2);
                entity.Property(e => e.AvailableQuantity).HasPrecision(18, 2);
                entity.Property(e => e.OriginalQuantity).HasPrecision(18, 2);
                entity.Property(e => e.Unit).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.ListedDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.PurchasedQuantity).HasPrecision(18, 2);

                entity.HasOne(d => d.Harvest)
                    .WithMany(p => p.CropListings)
                    .HasForeignKey(d => d.HarvestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Buyer)
                    .WithMany()
                    .HasForeignKey(d => d.BuyerId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // CropOrder mapping
            modelBuilder.Entity<CropOrder>(entity =>
            {
                entity.HasKey(e => e.OrderId);
                entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.PricePerUnit).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");
                entity.Property(e => e.GST).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.InvoiceNumber).HasMaxLength(100);
                entity.Property(e => e.DeliveryAddress).HasMaxLength(500);
                entity.Property(e => e.SpecialInstructions).HasMaxLength(500);
                entity.Property(e => e.DeclineReason).HasMaxLength(100);
                entity.Property(e => e.DeclineNotes).HasMaxLength(500);

                entity.HasOne(d => d.Buyer)
                    .WithMany()
                    .HasForeignKey(d => d.BuyerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CropOrder_Buyer");

                entity.HasOne(d => d.Farmer)
                    .WithMany()
                    .HasForeignKey(d => d.FarmerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CropOrder_Farmer");

                entity.HasOne(d => d.CropListing)
                    .WithMany()
                    .HasForeignKey(d => d.ListingId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CropOrder_CropListing");

                entity.HasOne(d => d.Harvest)
                    .WithMany()
                    .HasForeignKey(d => d.HarvestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CropOrder_Harvest");
            });

            // New Notification mapping
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.HasKey(e => e.NotificationId);
                entity.Property(e => e.Title).HasMaxLength(150);
                entity.Property(e => e.Message).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // New SensorReading mapping
            modelBuilder.Entity<SensorReading>(entity =>
            {
                entity.HasKey(e => e.ReadingId);
                entity.Property(e => e.SoilMoisture).HasPrecision(5, 2);
                entity.Property(e => e.SoilPH).HasPrecision(4, 2);
                entity.Property(e => e.Nitrogen).HasPrecision(5, 2);
                entity.Property(e => e.Phosphorus).HasPrecision(5, 2);
                entity.Property(e => e.Potassium).HasPrecision(5, 2);
                entity.Property(e => e.ElectricalConductivity).HasPrecision(5, 2);
                entity.Property(e => e.OrganicCarbon).HasPrecision(4, 2);

                entity.HasOne(d => d.Plot)
                    .WithMany()
                    .HasForeignKey(d => d.PlotId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // New SupportQuery mapping
            modelBuilder.Entity<SupportQuery>(entity =>
            {
                entity.HasKey(e => e.QueryId);
                entity.Property(e => e.QueryType).HasMaxLength(100);
                entity.Property(e => e.Title).HasMaxLength(150);
                entity.Property(e => e.Description).HasMaxLength(1000);
                entity.Property(e => e.Priority).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.AgronomistRecommendation).HasMaxLength(1000);
                entity.Property(e => e.FieldObservation).HasMaxLength(1000);
                entity.Property(e => e.ActionTaken).HasMaxLength(1000);
                entity.Property(e => e.ImprovementActions).HasMaxLength(1000);
                entity.Property(e => e.ImprovementExpectedBenefits).HasMaxLength(500);
                entity.Property(e => e.ImprovementStatus).HasMaxLength(50);

                entity.HasOne(d => d.Farmer)
                    .WithMany()
                    .HasForeignKey(d => d.FarmerId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Farm)
                    .WithMany()
                    .HasForeignKey(d => d.FarmId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.LandPlot)
                    .WithMany()
                    .HasForeignKey(d => d.PlotId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.AssignedToUser)
                    .WithMany()
                    .HasForeignKey(d => d.AssignedToUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // CultivationRequest mapping
            modelBuilder.Entity<CultivationRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.Property(e => e.CultivationArea).HasPrecision(18, 2);
                entity.Property(e => e.SoilPH).HasPrecision(4, 2);
                entity.Property(e => e.MoistureLevel).HasPrecision(5, 2);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Farmer)
                    .WithMany()
                    .HasForeignKey(d => d.FarmerId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Farm)
                    .WithMany()
                    .HasForeignKey(d => d.FarmId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.LandPlot)
                    .WithMany()
                    .HasForeignKey(d => d.PlotId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(d => d.Crop)
                    .WithMany()
                    .HasForeignKey(d => d.CropId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // AgronomistAnalysis mapping
            modelBuilder.Entity<AgronomistAnalysis>(entity =>
            {
                entity.HasKey(e => e.AnalysisId);
                entity.Property(e => e.SoilAnalysis).HasMaxLength(1000);
                entity.Property(e => e.WeatherAnalysis).HasMaxLength(1000);
                entity.Property(e => e.CropAnalysis).HasMaxLength(1000);
                entity.Property(e => e.PestAnalysis).HasMaxLength(1000);
                entity.Property(e => e.DiseaseAnalysis).HasMaxLength(1000);
                entity.Property(e => e.Recommendation).HasMaxLength(1000);
                entity.Property(e => e.Decision).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CultivationRequest)
                    .WithMany()
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Agronomist)
                    .WithMany()
                    .HasForeignKey(d => d.AgronomistId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // HarvestDecision mapping
            modelBuilder.Entity<HarvestDecision>(entity =>
            {
                entity.HasKey(e => e.DecisionId);
                entity.Property(e => e.CropHealthAnalysis).HasMaxLength(1000);
                entity.Property(e => e.PestAnalysis).HasMaxLength(1000);
                entity.Property(e => e.DiseaseAnalysis).HasMaxLength(1000);
                entity.Property(e => e.Recommendation).HasMaxLength(1000);
                entity.Property(e => e.Decision).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Harvest)
                    .WithMany()
                    .HasForeignKey(d => d.HarvestId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Agronomist)
                    .WithMany()
                    .HasForeignKey(d => d.AgronomistId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // AdminProfile mapping
            modelBuilder.Entity<AdminProfile>(entity =>
            {
                entity.HasKey(e => e.ProfileId);
                entity.Property(e => e.FirstName).HasMaxLength(50);
                entity.Property(e => e.LastName).HasMaxLength(50);
                entity.Property(e => e.EmployeeId).HasMaxLength(20);
                entity.Property(e => e.Department).HasMaxLength(100);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.Property(e => e.City).HasMaxLength(100);
                entity.Property(e => e.State).HasMaxLength(100);
                entity.Property(e => e.PinCode).HasMaxLength(10);

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_AdminProfile_Users_UserId");
            });

            
            // FIELD OFFICER MODULE MAPPINGS
            

            // YieldRecord mapping
            modelBuilder.Entity<YieldRecord>(entity =>
            {
                entity.HasKey(e => e.YieldId);
                entity.Property(e => e.Area).HasPrecision(12, 4);
                entity.Property(e => e.EstimatedYield).HasPrecision(14, 2);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.Property(e => e.SubmissionDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.Status).HasMaxLength(50);

                entity.HasOne(d => d.Farmer)
                    .WithMany()
                    .HasForeignKey(d => d.FarmerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_YieldRecord_Farmer");

                entity.HasOne(d => d.LandPlot)
                    .WithMany()
                    .HasForeignKey(d => d.PlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_YieldRecord_LandPlot");

                entity.HasOne(d => d.Crop)
                    .WithMany()
                    .HasForeignKey(d => d.CropId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_YieldRecord_Crop");

                entity.HasOne(d => d.SubmittedByUser)
                    .WithMany()
                    .HasForeignKey(d => d.SubmittedByUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_YieldRecord_Users");
            });

            // YieldPhoto mapping
            modelBuilder.Entity<YieldPhoto>(entity =>
            {
                entity.HasKey(e => e.PhotoId);
                entity.Property(e => e.FilePath).HasMaxLength(1000);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.YieldRecord)
                    .WithMany(p => p.YieldPhotos)
                    .HasForeignKey(d => d.YieldId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_YieldPhoto_YieldRecord");
            });

            // YieldValidation mapping
            modelBuilder.Entity<YieldValidation>(entity =>
            {
                entity.HasKey(e => e.ValidationId);
                entity.Property(e => e.ValidationDate).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.ValidationStatus).HasMaxLength(50);

                entity.HasOne(d => d.YieldRecord)
                    .WithMany(p => p.YieldValidations)
                    .HasForeignKey(d => d.YieldId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_YieldValidation_YieldRecord");

                entity.HasOne(d => d.FieldOfficer)
                    .WithMany()
                    .HasForeignKey(d => d.FieldOfficerUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_YieldValidation_Users");
            });

            // FieldOfficerAssignment mapping
            modelBuilder.Entity<FieldOfficerAssignment>(entity =>
            {
                entity.HasKey(e => e.AssignmentId);
                entity.Property(e => e.AssignedAt).HasDefaultValueSql("(getdate())");
                entity.Property(e => e.IsActive).HasDefaultValue(true);

                entity.HasOne(d => d.FieldOfficer)
                    .WithMany()
                    .HasForeignKey(d => d.FieldOfficerUserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FOA_Users");

                entity.HasOne(d => d.Farmer)
                    .WithMany()
                    .HasForeignKey(d => d.FarmerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FOA_Farmer");
            });

            // FieldVisit mapping
            modelBuilder.Entity<FieldVisit>(entity =>
            {
                entity.HasKey(e => e.VisitId);
                entity.Property(e => e.VisitTime).HasMaxLength(50);
                entity.Property(e => e.Status).HasMaxLength(50);
                entity.Property(e => e.Priority).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Farmer)
                    .WithMany()
                    .HasForeignKey(d => d.FarmerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FieldVisit_Farmer");

                entity.HasOne(d => d.LandPlot)
                    .WithMany()
                    .HasForeignKey(d => d.PlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FieldVisit_LandPlot");

                entity.HasOne(d => d.AssignedOfficer)
                    .WithMany()
                    .HasForeignKey(d => d.AssignedOfficerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_FieldVisit_Officer");
            });

            // VisitPhoto mapping
            modelBuilder.Entity<VisitPhoto>(entity =>
            {
                entity.HasKey(e => e.PhotoId);
                entity.Property(e => e.FilePath).HasMaxLength(1000);
                entity.Property(e => e.UploadedAt).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.FieldVisit)
                    .WithMany(p => p.VisitPhotos)
                    .HasForeignKey(d => d.VisitId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("FK_VisitPhoto_FieldVisit");
            });

            // Agronomist mapping
            modelBuilder.Entity<Agronomist>(entity =>
            {
                entity.HasKey(e => e.AgronomistId);
                entity.Property(e => e.FullName).HasMaxLength(100);
                entity.Property(e => e.MobileNumber).HasMaxLength(15);
                entity.Property(e => e.Specialization).HasMaxLength(100);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasIndex(e => e.MobileNumber).IsUnique().HasDatabaseName("UQ_Agronomist_MobileNumber");
                entity.HasIndex(e => e.UserId).IsUnique().HasDatabaseName("UQ_Agronomist_UserId");

                entity.HasOne(d => d.User)
                    .WithMany()
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Agronomist_Users");
            });

            // CultivationRequest mapping
            modelBuilder.Entity<CultivationRequest>(entity =>
            {
                entity.HasKey(e => e.RequestId);
                entity.Property(e => e.CultivationArea).HasColumnType("decimal(10, 2)");
                entity.Property(e => e.SoilPH).HasColumnType("decimal(4, 2)");
                entity.Property(e => e.MoistureLevel).HasColumnType("decimal(5, 2)");
                entity.Property(e => e.Status).HasMaxLength(50).HasDefaultValue("Pending");
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Farmer)
                    .WithMany()
                    .HasForeignKey(d => d.FarmerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CultivationRequest_Farmer");

                entity.HasOne(d => d.Farm)
                    .WithMany()
                    .HasForeignKey(d => d.FarmId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CultivationRequest_Farm");

                entity.HasOne(d => d.LandPlot)
                    .WithMany()
                    .HasForeignKey(d => d.PlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CultivationRequest_LandPlot");

                entity.HasOne(d => d.Crop)
                    .WithMany()
                    .HasForeignKey(d => d.CropId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_CultivationRequest_Crop");
            });

            // AgronomistAnalysis mapping
            modelBuilder.Entity<AgronomistAnalysis>(entity =>
            {
                entity.HasKey(e => e.AnalysisId);
                entity.Property(e => e.SoilAnalysis).HasMaxLength(1000);
                entity.Property(e => e.WeatherAnalysis).HasMaxLength(1000);
                entity.Property(e => e.CropAnalysis).HasMaxLength(1000);
                entity.Property(e => e.PestAnalysis).HasMaxLength(1000);
                entity.Property(e => e.DiseaseAnalysis).HasMaxLength(1000);
                entity.Property(e => e.Recommendation).HasMaxLength(1000);
                entity.Property(e => e.Decision).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.CultivationRequest)
                    .WithMany()
                    .HasForeignKey(d => d.RequestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AgronomistAnalysis_Request");

                entity.HasOne(d => d.Agronomist)
                    .WithMany()
                    .HasForeignKey(d => d.AgronomistId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_AgronomistAnalysis_Agronomist");
            });

            // HarvestDecision mapping
            modelBuilder.Entity<HarvestDecision>(entity =>
            {
                entity.HasKey(e => e.DecisionId);
                entity.Property(e => e.CropHealthAnalysis).HasMaxLength(1000);
                entity.Property(e => e.PestAnalysis).HasMaxLength(1000);
                entity.Property(e => e.DiseaseAnalysis).HasMaxLength(1000);
                entity.Property(e => e.Recommendation).HasMaxLength(1000);
                entity.Property(e => e.Decision).HasMaxLength(50);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("(getdate())");

                entity.HasOne(d => d.Harvest)
                    .WithMany()
                    .HasForeignKey(d => d.HarvestId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HarvestDecision_Harvest");

                entity.HasOne(d => d.Agronomist)
                    .WithMany()
                    .HasForeignKey(d => d.AgronomistId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_HarvestDecision_Agronomist");
            });

            
            // SEED DATA FOR CODE FIRST APPROACH
            
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "Farmer" },
                new Role { RoleId = 3, RoleName = "Buyer" },
                new Role { RoleId = 4, RoleName = "Agronomist" },
                new Role { RoleId = 5, RoleName = "Field Officer" },
                new Role { RoleId = 6, RoleName = "Cooperative Manager" }
            );

            // Seed Default Admin User
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "admin",
                    PasswordHash = "admin123", // In production, use hashed passwords
                    Email = "admin@smartfarm.com",
                    RoleId = 1,
                    IsActive = true,
                    IsDeleted = false,
                    IsBlocked = false,
                    CreatedAt = new DateTime(2024, 1, 1)
                }
            );

            // Seed Admin Profile
            modelBuilder.Entity<AdminProfile>().HasData(
                new AdminProfile
                {
                    ProfileId = 1,
                    UserId = 1,
                    FirstName = "System",
                    LastName = "Administrator",
                    EmployeeId = "ADMIN001",
                    Department = "IT"
                }
            );
        }

        // Helper method to guarantee marketplace columns exist in SQL Server DB
        public void EnsureMarketplaceColumnsExist()
        {
            try
            {
                Database.ExecuteSqlRaw(@"
                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CropListing' AND COLUMN_NAME = 'OriginalQuantity')
                    BEGIN
                        ALTER TABLE [CropListing] ADD [OriginalQuantity] DECIMAL(18,2) NULL;
                    END;
                    EXEC('UPDATE [CropListing] SET [OriginalQuantity] = [AvailableQuantity] WHERE [OriginalQuantity] IS NULL');

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'CropOrder' AND COLUMN_NAME = 'DeclineReason')
                    BEGIN
                        ALTER TABLE [CropOrder] ADD [DeclineReason] NVARCHAR(100) NULL, [DeclineNotes] NVARCHAR(500) NULL, [DeclinedDate] DATETIME NULL;
                    END;
                ");
            }
            catch
            {
                // Suppress if already existing or locked
            }
        }
    }
}
