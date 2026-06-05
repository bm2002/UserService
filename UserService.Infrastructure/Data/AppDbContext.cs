using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;

namespace UserService.Infrastructure.Data;

public sealed class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<BalanceHistory> BalanceHistories => Set<BalanceHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                  .HasColumnName("id");

            entity.Property(u => u.FullName)
                  .HasColumnName("full_name")
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(u => u.BirthDate)
                  .HasColumnName("birth_date")
                  .IsRequired();

            entity.Property(u => u.BirthPlace)
                  .HasColumnName("birth_place")
                  .IsRequired()
                  .HasMaxLength(500);

            entity.Property(u => u.Balance)
                  .HasColumnName("balance")
                  .HasPrecision(18, 2);

            entity.Property(u => u.RowVersion)
                  .IsRowVersion();
        });

        modelBuilder.Entity<BalanceHistory>(entity =>
        {
            entity.ToTable("balance_histories");

            entity.HasKey(b => b.Id);

            entity.Property(b => b.Id)
                  .HasColumnName("id");

            entity.Property(b => b.UserId)
                  .HasColumnName("user_id");

            entity.Property(b => b.Delta)
                  .HasColumnName("delta")
                  .IsRequired()
                  .HasPrecision(18, 2);

            entity.Property(b => b.BalanceAfter)
                  .HasColumnName("balance_after")
                  .IsRequired()
                  .HasPrecision(18, 2);

            entity.Property(b => b.ChangedAt)
                  .IsRequired()
                  .HasColumnName("changed_at");

            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(b => b.UserId)
                  .HasConstraintName("fk_balance_histories_users");
        });
    }
}