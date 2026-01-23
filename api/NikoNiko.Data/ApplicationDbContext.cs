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
    public DbSet<TeamInvitation> TeamInvitations { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

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

        // Configure TeamInvitation relationships
        modelBuilder.Entity<TeamInvitation>()
            .HasOne(ti => ti.Team)
            .WithMany() // A team can have many invitations, but we don't necessarily need a navigation property on the Team side for this example.
            .HasForeignKey(ti => ti.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TeamInvitation>()
            .HasOne(ti => ti.CreatorUser)
            .WithMany()
            .HasForeignKey(ti => ti.CreatorUserId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent deleting a user who created an invitation

        modelBuilder.Entity<TeamInvitation>()
            .HasOne(ti => ti.AcceptedByUser)
            .WithMany()
            .HasForeignKey(ti => ti.AcceptedByUserId)
            .IsRequired(false) // AcceptedByUser can be null
            .OnDelete(DeleteBehavior.SetNull); // Allow deleting a user who accepted an invitation

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

        modelBuilder.Entity<TeamInvitation>()
            .HasIndex(ti => ti.Token)
            .IsUnique();

        // Filtre global pour le soft delete
        modelBuilder.Entity<TeamInvitation>()
            .HasQueryFilter(ti => !ti.IsDeleted);

        // Add check constraint for Sprint dates
        modelBuilder.Entity<Sprint>()
            .ToTable(t => t.HasCheckConstraint("CK_Sprint_EndDate_After_StartDate", "\"EndDate\" > \"StartDate\""));
    }
}