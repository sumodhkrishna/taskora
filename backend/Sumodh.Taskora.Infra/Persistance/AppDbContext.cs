using Microsoft.EntityFrameworkCore;
using Sumodh.Taskora.Domain.Authentication;
using Sumodh.Taskora.Domain.Todos;
using Sumodh.Taskora.Domain.Users;

namespace Sumodh.Taskora.Infra.Persistance
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<TodoItem> TodoItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<User>().HasKey(x => x.Id);
            modelBuilder.Entity<User>().Property(x => x.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<User>().Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
            modelBuilder.Entity<User>().Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);
            modelBuilder.Entity<User>().Property(x => x.PasswordHash)
                .IsRequired();
            modelBuilder.Entity<User>().Property(x => x.CreatedAt)
                .IsRequired();
            modelBuilder.Entity<User>().HasIndex(x => x.Email)
                .IsUnique();
            modelBuilder.Entity<User>().Property(x => x.IsEmailVerified)
                .IsRequired()
                .HasDefaultValue(false);
            modelBuilder.Entity<User>().Property(x => x.EmailVerifiedAtUtc);
            modelBuilder.Entity<User>().Property(x => x.EmailVerificationTokenHash)
                .HasMaxLength(256);
            modelBuilder.Entity<User>().Property(x => x.EmailVerificationTokenExpiresAtUtc);
            modelBuilder.Entity<User>().Property(x => x.PasswordResetTokenHash)
                .HasMaxLength(256);
            modelBuilder.Entity<User>().Property(x => x.PasswordResetTokenExpiresAtUtc);
            modelBuilder.Entity<User>().Property(x => x.PasswordResetRequestedAtUtc);


            modelBuilder.Entity<TodoItem>().ToTable("TodoItems");
            modelBuilder.Entity<TodoItem>().HasKey(x => x.Id);
            modelBuilder.Entity<TodoItem>().Property(x => x.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<TodoItem>().Property(x => x.UserId)
                .IsRequired();
            modelBuilder.Entity<TodoItem>().Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);
            modelBuilder.Entity<TodoItem>().Property(x => x.Description)
                .HasMaxLength(2000);
            modelBuilder.Entity<TodoItem>().Property(x => x.Priority)
                .IsRequired()
                .HasConversion<int>();
            modelBuilder.Entity<TodoItem>().Property(x => x.ToBeCompletedByDateUtc);
            modelBuilder.Entity<TodoItem>().Property(x => x.IsCompleted)
                .IsRequired();
            modelBuilder.Entity<TodoItem>().Property(x => x.CreatedAtUtc)
                .IsRequired();
            modelBuilder.Entity<TodoItem>().Property(x => x.UpdatedAtUtc);
            modelBuilder.Entity<TodoItem>().Property(x => x.CompletedAtUtc);
            modelBuilder.Entity<TodoItem>().HasOne<Sumodh.Taskora.Domain.Users.User>()
                .WithMany(x => x.TodoItems)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<TodoItem>().HasIndex(x => x.UserId);
            modelBuilder.Entity<TodoItem>().HasIndex(x => new { x.UserId, x.IsCompleted });
            modelBuilder.Entity<TodoItem>().HasIndex(x => new { x.UserId, x.Priority });

            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");
            modelBuilder.Entity<RefreshToken>().HasKey(x => x.Id);  
            modelBuilder.Entity<RefreshToken>().Property(x => x.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<RefreshToken>().Property(x => x.UserId)
                .IsRequired();
            modelBuilder.Entity<RefreshToken>().Property(x => x.TokenHash)
                .IsRequired()
                .HasMaxLength(256);
            modelBuilder.Entity<RefreshToken>().Property(x => x.ExpiresAtUtc)
                .IsRequired();
            modelBuilder.Entity<RefreshToken>().Property(x => x.CreatedAtUtc)
                .IsRequired();
            modelBuilder.Entity<RefreshToken>().Property(x => x.IsRevoked)
                .IsRequired();
            modelBuilder.Entity<RefreshToken>().Property(x => x.RevokedAtUtc);
            modelBuilder.Entity<RefreshToken>().Property(x => x.ReplacedByTokenHash)
                .HasMaxLength(256);
            modelBuilder.Entity<RefreshToken>().HasIndex(x => x.TokenHash)
                .IsUnique();
            modelBuilder.Entity<RefreshToken>().HasIndex(x => x.UserId);
            modelBuilder.Entity<RefreshToken>().HasOne<Sumodh.Taskora.Domain.Users.User>()
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
