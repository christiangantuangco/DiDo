using DiDo.Models;
using Microsoft.EntityFrameworkCore;

namespace DiDo.Data;

public class DiDoDbContext(DbContextOptions<DiDoDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Entry> Entries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Entries)
            .WithOne()
            .HasForeignKey(e => e.UserId);

        base.OnModelCreating(modelBuilder);
    }
}