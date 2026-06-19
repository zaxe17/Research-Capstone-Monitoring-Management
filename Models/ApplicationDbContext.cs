using Microsoft.EntityFrameworkCore;

namespace monitoring_management.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
    public DbSet<Admin> Admins { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<ResearchPaper> ResearchPapers { get; set; }
    public DbSet<ResearchMember> ResearchMembers { get; set; }
    public DbSet<AcademicProgram> AcademicPrograms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>(e =>
        {
            e.ToTable("students");
            e.HasIndex(x => x.StudentNo).IsUnique();
            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<Admin>(e =>
        {
            e.ToTable("admin");
            e.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.HasIndex(x => x.CategoryName).IsUnique();
        });

        modelBuilder.Entity<ResearchPaper>(e =>
        {
            e.ToTable("research_papers");
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Year).HasColumnType("year");

            e.HasOne(x => x.Student)
                .WithMany(s => s.ResearchPapers)
                .HasForeignKey(x => x.UploadedBy)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Category)
                .WithMany(c => c.ResearchPapers)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<ResearchMember>(e =>
        {
            e.ToTable("research_members");
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(10);

            e.HasOne(x => x.ResearchPaper)
                .WithMany(p => p.ResearchMembers)
                .HasForeignKey(x => x.PaperId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Student)
                .WithMany(s => s.ResearchMembers)
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<AcademicProgram>(e =>
        {
            e.ToTable("academic_programs");
            e.HasIndex(x => x.Code).IsUnique();
        });
    }
}