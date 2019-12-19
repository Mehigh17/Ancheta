using Ancheta.Model.Data;
using Microsoft.EntityFrameworkCore;

namespace Ancheta.WebApi.Contexts
{
    public class ApplicationDbContext : DbContext
    {

        public DbSet<Poll> Polls { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Vote> Votes { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Poll>()
                        .HasMany(p => p.Answers)
                        .WithOne(a => a.OwnerPoll)
                        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answer>()
                        .HasOne(a => a.OwnerPoll)
                        .WithMany(p => p.Answers);

            modelBuilder.Entity<Vote>()
                        .HasOne(v => v.OwnerAnswer)
                        .WithMany(a => a.Votes);
        }

    }
}