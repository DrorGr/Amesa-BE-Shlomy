using AMESA_be.LotteryDevice.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AMESA_be.LotteryDevice.Data
{
    public class LotteryDbContext : DbContext
    {
        private readonly IConfiguration _configuration;

        public LotteryDbContext(DbContextOptions<LotteryDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public DbSet<Lottery> Lotteries { get; set; }
        public DbSet<LotteryUser> LotteryUsers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<LotteryWinners> LotteryWinners { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var connectionString = _configuration.GetConnectionString("LotteryDbConnection");
                optionsBuilder.UseNpgsql(connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Lottery>(entity =>
            {
                entity.ToTable("Lotteries");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id").IsRequired();
                entity.Property(e => e.Name).HasColumnName("Name").HasMaxLength(100);
                entity.Property(e => e.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
                entity.Property(e => e.ParticipantAmount).HasColumnName("ParticipantAmount");
                entity.Property(e => e.CurrentParticipants).HasColumnName("CurrentParticipants");
                entity.Property(e => e.Tenant).HasColumnName("Tenant");
                entity.Property(e => e.Status).HasColumnName("Status").HasDefaultValue(0);
                entity.Property(e => e.LastUpdateDate).HasColumnName("LastUpdateDate").HasDefaultValueSql("now()");
                entity.Property(e => e.CreatedDate).HasColumnName("createdDate").HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<LotteryUser>(entity =>
            {
                entity.ToTable("LotteryUsers");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id").IsRequired();
                entity.Property(e => e.LotteryId).HasColumnName("LotteryId");
                entity.Property(e => e.UserId).HasColumnName("UserId");
                entity.Property(e => e.RecommendedBy).HasColumnName("RecommendedBy");
                entity.Property(e => e.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
                entity.Property(e => e.LastUpdateDate).HasColumnName("LastUpdateDate").HasDefaultValueSql("now()");
                entity.Property(e => e.CreatedDate).HasColumnName("createdDate").HasDefaultValueSql("now()");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).HasColumnName("Id").IsRequired();
                entity.Property(e => e.UserName).HasColumnName("UserName").HasMaxLength(15).IsRequired();
                entity.HasIndex(e => e.UserName).IsUnique();
                entity.Property(e => e.FirstName).HasColumnName("FirstName").HasMaxLength(15).IsRequired();
                entity.Property(e => e.LastName).HasColumnName("LastName").HasMaxLength(20).IsRequired();
                entity.Property(e => e.Gender).HasColumnName("Gender");
                entity.Property(e => e.DateOfBirth).HasColumnName("DateOfBirth").IsRequired();
                entity.Property(e => e.Country).HasColumnName("Country").IsRequired();
                entity.Property(e => e.Address).HasColumnName("Address").HasColumnType("json");
                entity.Property(e => e.Tenant).HasColumnName("Tenant");
                entity.Property(e => e.IsActive).HasColumnName("IsActive").HasDefaultValue(true);
                entity.Property(e => e.Email).HasColumnName("Email").HasMaxLength(100);
                entity.Property(e => e.Phones).HasColumnName("Phones").HasColumnType("json");
                entity.Property(e => e.Password).HasColumnName("Password");
                entity.Property(e => e.UserId).HasColumnName("UserId").IsRequired();
                entity.Property(e => e.LastUpdateDate).HasColumnName("LastUpdateDate").HasDefaultValueSql("now()");
                entity.Property(e => e.CreatedDate).HasColumnName("createdDate").HasDefaultValueSql("now()");
            });
        }
    }
}