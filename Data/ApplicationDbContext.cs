using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Models;

namespace TechMoveGLMS.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<Contract> Contracts { get; set; }
        public DbSet<ServiceRequest> ServiceRequests { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>()
                .HasMany(client => client.Contracts)
                .WithOne(contract => contract.Client)
                .HasForeignKey(contract => contract.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasMany(contract => contract.ServiceRequests)
                .WithOne(request => request.Contract)
                .HasForeignKey(request => request.ContractId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contract>()
                .HasIndex(contract => new
                {
                    contract.Status,
                    contract.StartDate,
                    contract.EndDate
                });

            modelBuilder.Entity<ServiceRequest>()
                .Property(request => request.CostUsd)
                .HasPrecision(18, 2);

            modelBuilder.Entity<ServiceRequest>()
                .Property(request => request.ExchangeRate)
                .HasPrecision(18, 4);

            modelBuilder.Entity<ServiceRequest>()
                .Property(request => request.Cost)
                .HasPrecision(18, 2);
        }
    }
}