using Microsoft.EntityFrameworkCore;

using NikoNiko.Core.Models;

namespace NikoNiko.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamUser> TeamUsers { get; set; }
    public DbSet<Sprint> Sprints { get; set; }
    public DbSet<MoodEntry> MoodEntries { get; set; }
    public DbSet<Badge> Badges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the many-to-many relationship between User and Team through TeamUser
        modelBuilder.Entity<TeamUser>()
            .HasKey(tu => new { tu.UserId, tu.TeamId });

        modelBuilder.Entity<TeamUser>()
            .HasOne(tu => tu.User)
            .WithMany(u => u.TeamUsers)
            .HasForeignKey(tu => tu.UserId);

        modelBuilder.Entity<TeamUser>()
            .HasOne(tu => tu.Team)
            .WithMany(t => t.TeamUsers)
            .HasForeignKey(tu => tu.TeamId);

        // Configure the one-to-many relationship for Team Admin
        modelBuilder.Entity<Team>()
            .HasOne(t => t.Admin)
            .WithMany()
            .HasForeignKey(t => t.AdminId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a user who is an admin

        // Add unique constraints
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.OAuthId)
            .IsUnique();

        modelBuilder.Entity<Team>()
            .HasIndex(t => t.Name)
            .IsUnique();

        // Add check constraint for Sprint dates
        modelBuilder.Entity<Sprint>()
            .ToTable(t => t.HasCheckConstraint("CK_Sprint_EndDate_After_StartDate", "\"EndDate\" > \"StartDate\""));

        // Configure MoodType enum to be stored as string
        modelBuilder.Entity<MoodEntry>()
            .Property(me => me.Mood)
            .HasConversion<string>();
    }
}