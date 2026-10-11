using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    public class ServiceRequestCrudTests
    {
        [Fact]
        public async Task UpdateAsync_WhenContractIsActive_UpdatesRequest()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(ContractStatus.Active);

            var service = new ServiceRequestService(context);

            var updatedRequest = new ServiceRequest
            {
                ServiceRequestId = 1,
                ContractId = 1,
                Description = "Updated office move",
                CostUsd = 250,
                ExchangeRate = 18.50m,
                Cost = 4625,
                Status = ServiceRequestStatus.Completed
            };

            await service.UpdateAsync(updatedRequest);

            context.ChangeTracker.Clear();

            ServiceRequest? savedRequest =
                await context.ServiceRequests
                    .SingleOrDefaultAsync(request =>
                        request.ServiceRequestId == 1);

            Assert.NotNull(savedRequest);
            Assert.Equal("Updated office move", savedRequest.Description);
            Assert.Equal(250, savedRequest.CostUsd);
            Assert.Equal(18.50m, savedRequest.ExchangeRate);
            Assert.Equal(4625, savedRequest.Cost);
            Assert.Equal(
                ServiceRequestStatus.Completed,
                savedRequest.Status);
        }

        [Fact]
        public async Task UpdateAsync_WhenContractIsNotActive_ThrowsAndKeepsRequestUnchanged()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(ContractStatus.OnHold);

            var service = new ServiceRequestService(context);

            var updatedRequest = new ServiceRequest
            {
                ServiceRequestId = 1,
                ContractId = 1,
                Description = "Updated office move",
                CostUsd = 250,
                ExchangeRate = 18.50m,
                Cost = 4625,
                Status = ServiceRequestStatus.Completed
            };

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UpdateAsync(updatedRequest));

            ServiceRequest? savedRequest =
                await context.ServiceRequests
                    .SingleOrDefaultAsync(request =>
                        request.ServiceRequestId == 1);

            Assert.NotNull(savedRequest);
            Assert.Equal("Original office move", savedRequest.Description);
            Assert.Equal(ServiceRequestStatus.Pending, savedRequest.Status);
        }

        [Fact]
        public async Task UpdateAsync_WhenRequestDoesNotExist_ThrowsKeyNotFoundException()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(ContractStatus.Active);

            var service = new ServiceRequestService(context);

            var request = new ServiceRequest
            {
                ServiceRequestId = 999,
                ContractId = 1,
                Description = "Missing request",
                CostUsd = 100,
                ExchangeRate = 18.25m,
                Cost = 1825,
                Status = ServiceRequestStatus.Pending
            };

            KeyNotFoundException exception =
                await Assert.ThrowsAsync<KeyNotFoundException>(
                    () => service.UpdateAsync(request));

            Assert.Equal(
                "The service request could not be found.",
                exception.Message);
        }

        [Fact]
        public async Task DeleteAsync_WhenRequestExists_DeletesRequest()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(ContractStatus.Active);

            var service = new ServiceRequestService(context);

            await service.DeleteAsync(1);

            Assert.False(
                await context.ServiceRequests.AnyAsync(request =>
                    request.ServiceRequestId == 1));
        }

        [Fact]
        public async Task DeleteAsync_WhenRequestDoesNotExist_DoesNothing()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(ContractStatus.Active);

            var service = new ServiceRequestService(context);

            await service.DeleteAsync(999);

            Assert.Single(await context.ServiceRequests.ToListAsync());
        }

        [Fact]
        public async Task ExistsAsync_WhenRequestExists_ReturnsTrue()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(ContractStatus.Active);

            var service = new ServiceRequestService(context);

            bool exists = await service.ExistsAsync(1);

            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WhenRequestDoesNotExist_ReturnsFalse()
        {
            using ApplicationDbContext context =
                await CreateContextAsync(ContractStatus.Active);

            var service = new ServiceRequestService(context);

            bool exists = await service.ExistsAsync(999);

            Assert.False(exists);
        }

        private static async Task<ApplicationDbContext> CreateContextAsync(
            ContractStatus contractStatus)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"ServiceRequestCrudTests_{Guid.NewGuid()}")
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
                StartDate = today.AddDays(-1),
                EndDate = today.AddDays(1),
                Status = contractStatus,
                ServiceLevel = "Standard"
            };

            var request = new ServiceRequest
            {
                ServiceRequestId = 1,
                ContractId = contract.ContractId,
                Contract = contract,
                Description = "Original office move",
                CostUsd = 100,
                ExchangeRate = 18.25m,
                Cost = 1825,
                Status = ServiceRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            context.Clients.Add(client);
            context.Contracts.Add(contract);
            context.ServiceRequests.Add(request);

            await context.SaveChangesAsync();

            return context;
        }
    }
}
