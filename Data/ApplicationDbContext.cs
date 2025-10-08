using LinkCare_IT15.Models;
using LinkCare_IT15.Models.AdminModel;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace LinkCare_IT15.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

       
        public DbSet<Consultation> Consultations { get; set; }  // ✅ singular
        public DbSet<Appointment> Appointments { get; set; }

        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Billing> Billings { get; set; }
        public DbSet<Doctor> Doctors { get; set; }

        public DbSet<EquipmentModel> Equipments { get; set; }
        public DbSet<ConsumableModel> Consumables { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; }

        public DbSet<ConsumableBatch> ConsumableBatches { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Consultation>()
                .HasOne(c => c.Doctor)
                .WithMany(u => u.ConsultationsAsDoctor)
                .HasForeignKey(c => c.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Consultation>()
                .HasOne(c => c.Patient)
                .WithMany(u => u.ConsultationsAsPatient)
                .HasForeignKey(c => c.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany()
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<MaintenanceLog>()
               .HasOne(m => m.Equipment)
               .WithMany(e => e.MaintenanceLogs)
               .HasForeignKey(m => m.EquipmentId)
               .OnDelete(DeleteBehavior.Cascade);


           builder.Entity<ConsumableBatch>()
           .HasOne(cb => cb.Consumable)
           .WithMany(c => c.Batches)
           .HasForeignKey(cb => cb.ConsumableId)
       .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<EquipmentModel>(entity =>
            {
                entity.ToTable("Equipments");
                entity.HasKey(e => e.EquipmentId);

                entity.Property(e => e.EquipmentName).IsRequired();
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.PurchaseCost).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Status).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.LastMaintenanceDate).IsRequired(false);
                entity.Property(e => e.NextMaintenanceDate).IsRequired(false);
                entity.Property(e => e.ImageData).HasColumnType("VARBINARY(MAX)").IsRequired(false);
            });


        builder.Entity<ConsumableModel>(entity =>
            {
                entity.ToTable("Consumables");
                entity.HasKey(c => c.ConsumableId);

                entity.Property(c => c.ConsumableName).IsRequired();
                entity.Property(c => c.Category).IsRequired();
                entity.Property(c => c.UnitCost).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(c => c.Status).IsRequired();
                entity.Property(c => c.ImageData).HasColumnType("VARBINARY(MAX)").IsRequired(false);
            });
        }

    }
}