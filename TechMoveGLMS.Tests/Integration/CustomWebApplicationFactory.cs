using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TechMoveGLMS.Api.Data;

namespace TechMoveGLMS.Tests.Integration
{
    public class CustomWebApplicationFactory
        : WebApplicationFactory<Program>
    {
        private readonly string _databaseName =
            $"TechMoveGLMSTestDb-{Guid.NewGuid()}";

        protected override void ConfigureWebHost(
            IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<ApplicationDbContext>();

                services.RemoveAll<
                    DbContextOptions<ApplicationDbContext>>();

                services.RemoveAll<
                    IDbContextOptionsConfiguration<ApplicationDbContext>>();

                services.AddDbContext<ApplicationDbContext>(
                    options =>
                    {
                        options.UseInMemoryDatabase(_databaseName);
                    });

                using var serviceProvider =
                    services.BuildServiceProvider();

                using var scope =
                    serviceProvider.CreateScope();

                var context = scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

                context.Database.EnsureCreated();
            });
        }
    }
}