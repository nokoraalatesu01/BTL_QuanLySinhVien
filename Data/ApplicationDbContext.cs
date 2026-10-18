using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BTL_QuanLySinhVien.Models;

namespace BTL_QuanLySinhVien.Data
{
   
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        
        public DbSet<Student> Students { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<Teacher> Teachers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Major>()
                .HasIndex(major => major.Name)
                .IsUnique();

            builder.Entity<Student>()
                .HasOne(student => student.Major)
                .WithMany(major => major.Students)
                .HasForeignKey(student => student.MajorId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(user => user.StudentId).HasMaxLength(450);
                entity.Property(user => user.TeacherId).HasMaxLength(450);

                entity.HasOne<Student>()
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(user => user.StudentId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne<Teacher>()
                    .WithOne()
                    .HasForeignKey<ApplicationUser>(user => user.TeacherId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(user => user.StudentId)
                    .IsUnique()
                    .HasFilter("[StudentId] IS NOT NULL");

                entity.HasIndex(user => user.TeacherId)
                    .IsUnique()
                    .HasFilter("[TeacherId] IS NOT NULL");

                entity.ToTable("AspNetUsers", table =>
                    table.HasCheckConstraint(
                        "CK_AspNetUsers_ProfileLinkExclusive",
                        "[StudentId] IS NULL OR [TeacherId] IS NULL"));
            });
        }
    }
}