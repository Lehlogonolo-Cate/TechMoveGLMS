using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Client> Clients { get; set; }
            = null!;

        public DbSet<Contract> Contracts { get; set; }
            = null!;

        public DbSet<ServiceRequest> ServiceRequests { get; set; }
            = null!;
    }
}
