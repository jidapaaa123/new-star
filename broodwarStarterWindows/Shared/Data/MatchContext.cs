using Microsoft.EntityFrameworkCore;
using Shared.Models;

namespace Shared.Data
{
    public class MatchContext : DbContext
    {
        public DbSet<Match> Matches { get; set; } = null!;
        public DbSet<GameEvent> GameEvents { get; set; } = null!;

        public MatchContext(DbContextOptions<MatchContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Match>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.StartTime)
                    .IsRequired();

                entity.Property(e => e.EndTime);

                entity.Property(e => e.FinalWorkerCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.FinalMilitaryCount)
                    .HasDefaultValue(0);

                entity.Property(e => e.FinalMinerals)
                    .HasDefaultValue(0);

                entity.Property(e => e.FinalGas)
                    .HasDefaultValue(0);

                entity.Property(e => e.DidExpand)
                    .HasDefaultValue(false);

                entity.Property(e => e.UpgradesCompleted)
                    .HasDefaultValue(0);

                entity.Property(e => e.Result)
                    .IsRequired()
                    .HasDefaultValue("Ongoing");
            });

            modelBuilder.Entity<GameEvent>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Id)
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.MatchId)
                    .IsRequired();

                entity.Property(e => e.Timestamp)
                    .IsRequired();

                entity.Property(e => e.EventType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasOne<Match>()
                    .WithMany()
                    .HasForeignKey(e => e.MatchId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
