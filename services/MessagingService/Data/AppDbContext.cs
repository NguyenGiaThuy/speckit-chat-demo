using MessagingService.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace MessagingService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Message> Messages => Set<Message>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.HasIndex(m => m.ChannelId);
            entity.HasIndex(m => new { m.ChannelId, m.CreatedAt });
            entity.Property(m => m.Content).IsRequired().HasMaxLength(2000);

            entity.HasOne(m => m.ReplyTo)
                .WithMany()
                .HasForeignKey(m => m.ReplyToId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
