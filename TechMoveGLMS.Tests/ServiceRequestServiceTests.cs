using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    public class ServiceRequestServiceTests
    {
        [Theory]
        [InlineData(ContractStatus.Expired)]
        [InlineData(ContractStatus.OnHold)]
        [InlineData(ContractStatus.Draft)]
        public async Task CreateAsync_WhenContractIsNotActive_ThrowsAndDoesNotSave(
            ContractStatus contractStatus)
        {
            using ApplicationDbContext context =
                await CreateContextWithContractAsync(contractStatus);

            var service = new ServiceRequestService(context);
            var request = CreateServiceRequest();

            InvalidOperationException exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.CreateAsync(request));

            Assert.Equal(
                "Service requests can only be created for active contracts.",
                exception.Message);

            Assert.Empty(await context.ServiceRequests.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_WhenActiveContractHasNotStarted_ThrowsAndDoesNotSave()
        {
            using ApplicationDbContext context =
                await CreateContextWithContractAsync(
                    ContractStatus.Active,
                    startDate: DateTime.Today.AddDays(1));

            var service = new ServiceRequestService(context);
            var request = CreateServiceRequest();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(request));

            Assert.Empty(await context.ServiceRequests.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_WhenActiveContractHasEnded_ThrowsAndDoesNotSave()
        {
            using ApplicationDbContext context =
                await CreateContextWithContractAsync(
                    ContractStatus.Active,
                    endDate: DateTime.Today.AddDays(-1));

            var service = new ServiceRequestService(context);
            var request = CreateServiceRequest();

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.CreateAsync(request));

            Assert.Empty(await context.ServiceRequests.ToListAsync());
        }

        [Fact]
        public async Task CreateAsync_WhenContractIsActive_SavesServiceRequest()
        {
            using ApplicationDbContext context =
                await CreateContextWithContractAsync(ContractStatus.Active);

            var service = new ServiceRequestService(context);
            var request = CreateServiceRequest();

            await service.CreateAsync(request);

            ServiceRequest? savedRequest =
                await context.ServiceRequests
                    .SingleOrDefaultAsync();

            Assert.NotNull(savedRequest);
            Assert.Equal(1, savedRequest.ContractId);
            Assert.Equal("Move office equipment", savedRequest.Description);
            Assert.NotEqual(default, savedRequest.CreatedAt);
        }

        private static ServiceRequest CreateServiceRequest()
        {
            return new ServiceRequest
            {
                ContractId = 1,
                Description = "Move office equipment",
                CostUsd = 100,
                ExchangeRate = 18.25m,
                Cost = 1825,
                Status = ServiceRequestStatus.Pending
            };
        }

        private static async Task<ApplicationDbContext>
            CreateContextWithContractAsync(
                ContractStatus status,
                DateTime? startDate = null,
                DateTime? endDate = null)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"ServiceRequestTests_{Guid.NewGuid()}")
                .Options;

            var context = new ApplicationDbContext(options);

            DateTime today = DateTime.Today;

            var client = new Client
            {
                ClientId = 1,
                Name = "Test Client",
                ContactDetails = "test@example.com",
                Region = "Gauteng"
            };

            var contract = new Contract
            {
                ContractId = 1,
                ClientId = client.ClientId,
                Client = client,
                StartDate = startDate ?? today.AddDays(-1),
                EndDate = endDate ?? today.AddDays(1),
                Status = status,
                ServiceLevel = "Standard"
            };

            context.Clients.Add(client);
            context.Contracts.Add(contract);

            await context.SaveChangesAsync();

            return context;
        }
    }
}