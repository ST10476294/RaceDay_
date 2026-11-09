using Microsoft.EntityFrameworkCore;

using RaceDay.Api.Models;
using System.Collections.Generic;
using System.Reflection.Emit;



namespace RaceDay.Api.Data

{

    public class RaceDayDbContext : DbContext

    {

        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options)

            : base(options)

        {

        }



        public DbSet<User> Users { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Models.Route> Routes { get; set; }

        public DbSet<Enrolment> Enrolments { get; set; }

        public DbSet<Result> Results { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)

        {

            base.OnModelCreating(modelBuilder);
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    if (property.ClrType == typeof(string)) property.SetIsUnicode(false);
                    if (property.ClrType == typeof(DateTime)) property.SetColumnType("datetime");
                }
                foreach (var foreignKey in entity.GetForeignKeys()) foreignKey.DeleteBehavior = DeleteBehavior.NoAction;
            }
            modelBuilder.Entity<User>().Property(x => x.Role).HasDefaultValue("Participant");
            modelBuilder.Entity<User>().Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Category>().Property(x => x.MaxParticipants).HasDefaultValue(100);
            modelBuilder.Entity<Category>().Property(x => x.EntryFee).HasDefaultValue(0m);
            modelBuilder.Entity<Enrolment>().Property(x => x.EnrolmentDate).HasDefaultValueSql("GETDATE()");
            modelBuilder.Entity<Enrolment>().Property(x => x.Status).IsRequired().HasDefaultValue("Confirmed");
            modelBuilder.Entity<Result>().Property(x => x.Status).IsRequired().HasDefaultValue("Finished");




            // Unique constraints matching your Part 1 SQL script

            modelBuilder.Entity<User>()

                .HasIndex(u => u.Email)

                .IsUnique();



            modelBuilder.Entity<Enrolment>()

                .HasIndex(e => new { e.ParticipantId, e.CategoryId })

                .IsUnique();



            modelBuilder.Entity<Result>()

                .HasIndex(r => r.EnrolmentId)

                .IsUnique();



            // Role check constraint

            modelBuilder.Entity<User>()

                .ToTable(t => t.HasCheckConstraint("CK_Users_Role", "[Role] IN ('Organiser', 'Participant')"));



            // Discipline check constraint

            modelBuilder.Entity<Event>()

                .ToTable(t => t.HasCheckConstraint("CK_Events_Discipline", "[Discipline] IN ('Running', 'Walking', 'Cycling')"));



            // Enrolment status check constraint

            modelBuilder.Entity<Enrolment>()

                .ToTable(t => t.HasCheckConstraint("CK_Enrolments_Status", "[Status] IN ('Confirmed', 'Cancelled', 'Pending')"));



            // Result status check constraint

            modelBuilder.Entity<Result>()

                .ToTable(t => t.HasCheckConstraint("CK_Results_Status", "[Status] IN ('Finished', 'DNF', 'DSQ')"));

        }

    }

}