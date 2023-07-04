using Microsoft.EntityFrameworkCore;
using BettingDataProvider.Models;

namespace BettingDataProvider.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Sport>()
                .HasIndex(e => e.SportId)
                .IsUnique();

            modelBuilder.Entity<Event>()
               .HasIndex(e => e.EventId)
               .IsUnique();

            modelBuilder.Entity<Match>()
               .HasIndex(e => e.MatchId)
               .IsUnique();

            modelBuilder.Entity<Bet>()
               .HasIndex(e => e.BetId)
               .IsUnique();

            modelBuilder.Entity<Odd>()
               .HasIndex(e => e.OddId)
               .IsUnique();
        }
        public DbSet<Sport> Sports { get; set; }

        public DbSet<Event> Events { get; set; }

        public DbSet<Match> Matches { get; set; }

        public DbSet<Bet> Bets { get; set; }

        public DbSet<Odd> Odds { get; set; }
    }
}
