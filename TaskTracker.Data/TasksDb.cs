using Microsoft.EntityFrameworkCore;
using TaskTracker.Data.Models;

namespace TaskTracker.Data;

/// <summary>
/// EF Core DBContext
/// </summary>
public class TasksDb : DbContext
{
    public TasksDb(DbContextOptions<TasksDb> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<AppUser> Users => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // AppUser
        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired();
        });

        // TaskItem
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);

            e.HasOne(t => t.User)
             .WithMany(u => u.Tasks)
             .HasForeignKey(t => t.CreatedBy)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
