using Microsoft.EntityFrameworkCore;
using Sprouty.Api.Models;

namespace Sprouty.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Challenge> Challenges => Set<Challenge>();
    public DbSet<ChallengeEntry> ChallengeEntries => Set<ChallengeEntry>();
    public DbSet<Institution> Institutions => Set<Institution>();
    public DbSet<InstitutionMember> InstitutionMembers => Set<InstitutionMember>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Applause> Applauses => Set<Applause>();
    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<User>().HasIndex(u => u.Email).IsUnique();

        b.Entity<Post>()
            .HasOne(p => p.User)
            .WithMany(u => u.Posts)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Post>()
            .HasOne(p => p.Challenge)
            .WithMany(c => c.Posts)
            .HasForeignKey(p => p.ChallengeId)
            .OnDelete(DeleteBehavior.SetNull);

        b.Entity<Challenge>()
            .HasOne(c => c.User)
            .WithMany(u => u.Challenges)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<ChallengeEntry>()
            .HasOne(e => e.Challenge)
            .WithMany(c => c.Entries)
            .HasForeignKey(e => e.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<InstitutionMember>()
            .HasIndex(m => new { m.InstitutionId, m.UserId })
            .IsUnique();

        b.Entity<InstitutionMember>()
            .HasOne(m => m.User)
            .WithMany(u => u.InstitutionMemberships)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<InstitutionMember>()
            .HasOne(m => m.Institution)
            .WithMany(i => i.Members)
            .HasForeignKey(m => m.InstitutionId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Comment>()
            .HasOne(c => c.Post)
            .WithMany(p => p.Comments)
            .HasForeignKey(c => c.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Comment>()
            .HasOne(c => c.User).WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Applause>()
            .HasIndex(a => new { a.PostId, a.UserId })
            .IsUnique();

        b.Entity<Applause>()
            .HasOne(a => a.Post)
            .WithMany(p => p.Applauses)
            .HasForeignKey(a => a.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Applause>()
            .HasOne(a => a.User).WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Follow relationships
        b.Entity<Follow>()
            .HasIndex(f => new { f.FollowerId, f.FollowingId })
            .IsUnique();

        b.Entity<Follow>()
            .HasOne(f => f.Follower)
            .WithMany()
            .HasForeignKey(f => f.FollowerId)
            .OnDelete(DeleteBehavior.Restrict);

        b.Entity<Follow>()
            .HasOne(f => f.Following)
            .WithMany()
            .HasForeignKey(f => f.FollowingId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
