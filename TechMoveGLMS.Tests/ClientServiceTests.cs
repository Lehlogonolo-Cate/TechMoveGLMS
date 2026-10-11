using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    public class ClientServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsClientsInNameOrder()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            List<Client> clients = await service.GetAllAsync();

            Assert.Equal(2, clients.Count);
            Assert.Equal("Alpha Retail", clients[0].Name);
            Assert.Equal("Zeta Logistics", clients[1].Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenClientExists_ReturnsClientWithContracts()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(includeContract: true);

            var service = new ClientService(context);

            Client? client = await service.GetByIdAsync(1);

            Assert.NotNull(client);
            Assert.Equal("Zeta Logistics", client.Name);
            Assert.Single(client.Contracts);
        }

        [Fact]
        public async Task GetByIdAsync_WhenClientDoesNotExist_ReturnsNull()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            Client? client = await service.GetByIdAsync(999);

            Assert.Null(client);
        }

        [Fact]
        public async Task CreateAsync_SavesAndReturnsClient()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            var newClient = new Client
            {
                Name = "New Client",
                ContactDetails = "newclient@example.com",
                Region = "Limpopo"
            };

            Client createdClient = await service.CreateAsync(newClient);

            Assert.True(createdClient.ClientId > 0);

            Client? savedClient = await context.Clients
                .SingleOrDefaultAsync(client =>
                    client.ClientId == createdClient.ClientId);

            Assert.NotNull(savedClient);
            Assert.Equal("New Client", savedClient.Name);
        }

        [Fact]
        public async Task UpdateAsync_WhenClientExists_UpdatesAndReturnsTrue()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            // Clear the seeded entities from tracking so the service can
            // attach the updated client instance with the same ID.
            context.ChangeTracker.Clear();

            var updatedClient = new Client
            {
                ClientId = 1,
                Name = "Updated Logistics",
                ContactDetails = "updated@example.com",
                Region = "North West"
            };

            bool result = await service.UpdateAsync(updatedClient);

            Assert.True(result);

            Client? savedClient = await context.Clients
                .AsNoTracking()
                .SingleOrDefaultAsync(client => client.ClientId == 1);

            Assert.NotNull(savedClient);
            Assert.Equal("Updated Logistics", savedClient.Name);
            Assert.Equal("updated@example.com", savedClient.ContactDetails);
            Assert.Equal("North West", savedClient.Region);
        }

        [Fact]
        public async Task UpdateAsync_WhenClientDoesNotExist_ReturnsFalse()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            var missingClient = new Client
            {
                ClientId = 999,
                Name = "Missing Client",
                ContactDetails = "missing@example.com",
                Region = "Gauteng"
            };

            bool result = await service.UpdateAsync(missingClient);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenClientHasNoContracts_DeletesAndReturnsTrue()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            bool result = await service.DeleteAsync(1);

            Assert.True(result);
            Assert.False(
                await context.Clients.AnyAsync(client => client.ClientId == 1));
        }

        [Fact]
        public async Task DeleteAsync_WhenClientDoesNotExist_ReturnsFalse()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            bool result = await service.DeleteAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_WhenClientHasContracts_ReturnsFalseAndKeepsClient()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(includeContract: true);

            var service = new ClientService(context);

            bool result = await service.DeleteAsync(1);

            Assert.False(result);
            Assert.True(
                await context.Clients.AnyAsync(client => client.ClientId == 1));
            Assert.True(
                await context.Contracts.AnyAsync(contract => contract.ContractId == 1));
        }

        [Fact]
        public async Task ExistsAsync_WhenClientExists_ReturnsTrue()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            bool exists = await service.ExistsAsync(1);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WhenClientDoesNotExist_ReturnsFalse()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ClientService(context);

            bool exists = await service.ExistsAsync(999);

            Assert.False(exists);
        }

        private static async Task<ApplicationDbContext> CreateContextAsync(
            bool includeContract = false)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"ClientServiceTests_{Guid.NewGuid()}")
                .Options;

            var context = new ApplicationDbContext(options);

            var firstClient = new Client
            {
                ClientId = 1,
                Name = "Zeta Logistics",
                ContactDetails = "zeta@example.com",
                Region = "Gauteng"
            };

            var secondClient = new Client
            {
                ClientId = 2,
                Name = "Alpha Retail",
                ContactDetails = "alpha@example.com",
                Region = "Western Cape"
            };

            context.Clients.AddRange(firstClient, secondClient);

            if (includeContract)
            {
                DateTime today = DateTime.Today;

                var contract = new Contract
                {
                    ContractId = 1,
                    ClientId = firstClient.ClientId,
                    Client = firstClient,
                    StartDate = today.AddDays(-1),
                    EndDate = today.AddDays(30),
                    Status = ContractStatus.Active,
                    ServiceLevel = "Standard"
                };

                context.Contracts.Add(contract);
            }

            await context.SaveChangesAsync();

            return context;
        }
    }
}
