using Microsoft.EntityFrameworkCore;
using QRCodeManager.Domain.Entities;

namespace QRCodeManager.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<QrHistory> QrHistories => Set<QrHistory>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<QrHistory>(entity =>
        {
            entity.ToTable("QrHistories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.JsonData).IsRequired();
            entity.Property(x => x.QrImagePath).HasMaxLength(500);
            entity.Property(x => x.CreatedDate).IsRequired();
            entity.Property(x => x.QrType).HasConversion<int>().IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
            entity.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => x.Email).IsUnique();
        });
    }
}
