using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    public class ServiceRequestQueryTests
    {
        [Fact]
        public async Task GetAllAsync_WhenStatusIsProvided_ReturnsOnlyMatchingRequests()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ServiceRequestService(context);

            List<ServiceRequest> results = (await service.GetAllAsync(
                status: ServiceRequestStatus.Approved)).ToList();

            ServiceRequest result = Assert.Single(results);
            Assert.Equal(2, result.ServiceRequestId);
            Assert.Equal(ServiceRequestStatus.Approved, result.Status);
        }

        [Fact]
        public async Task GetAllAsync_WhenContractIdIsProvided_ReturnsOnlyMatchingRequests()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ServiceRequestService(context);

            List<ServiceRequest> results = (await service.GetAllAsync(
                contractId: 1)).ToList();

            ServiceRequest result = Assert.Single(results);
            Assert.Equal(1, result.ContractId);
        }

        [Theory]
        [InlineData("office equipment", 1)]
        [InlineData("Premium", 2)]
        [InlineData("Blue Sky Retail", 2)]
        public async Task GetAllAsync_WhenSearchTermMatchesRequestOrRelatedData_ReturnsMatch(
            string searchTerm,
            int expectedRequestId)
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ServiceRequestService(context);

            List<ServiceRequest> results = (await service.GetAllAsync(
                searchTerm: searchTerm)).ToList();

            ServiceRequest result = Assert.Single(results);
            Assert.Equal(expectedRequestId, result.ServiceRequestId);
        }

        [Fact]
        public async Task GetAllAsync_OrdersRequestsByCreatedAtDescending()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ServiceRequestService(context);

            List<ServiceRequest> results = (await service.GetAllAsync()).ToList();

            Assert.Equal(2, results.Count);
            Assert.Equal(2, results[0].ServiceRequestId);
            Assert.Equal(1, results[1].ServiceRequestId);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRequestExists_ReturnsRequestWithContractAndClient()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ServiceRequestService(context);

            ServiceRequest? result = await service.GetByIdAsync(1);

            Assert.NotNull(result);
            Assert.Equal(1, result.ServiceRequestId);
            Assert.NotNull(result.Contract);
            Assert.NotNull(result.Contract.Client);
            Assert.Equal("Northwind Logistics", result.Contract.Client.Name);
        }

        [Fact]
        public async Task GetByIdAsync_WhenRequestDoesNotExist_ReturnsNull()
        {
            using ApplicationDbContext context = await CreateContextAsync();
            var service = new ServiceRequestService(context);

            ServiceRequest? result = await service.GetByIdAsync(999);

            Assert.Null(result);
        }

        private static async Task<ApplicationDbContext> CreateContextAsync()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"ServiceRequestQueryTests_{Guid.NewGuid()}")
                .Options;

            var context = new ApplicationDbContext(options);

            var firstClient = new Client
            {
                ClientId = 1,
                Name = "Northwind Logistics",
                ContactDetails = "northwind@example.com",
                Region = "Gauteng"
            };

            var secondClient = new Client
            {
                ClientId = 2,
                Name = "Blue Sky Retail",
                ContactDetails = "bluesky@example.com",
                Region = "Western Cape"
            };

            DateTime today = DateTime.Today;

            var firstContract = new Contract
            {
                ContractId = 1,
                ClientId = firstClient.ClientId,
                Client = firstClient,
                StartDate = today.AddDays(-30),
                EndDate = today.AddDays(30),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };

            var secondContract = new Contract
            {
                ContractId = 2,
                ClientId = secondClient.ClientId,
                Client = secondClient,
                StartDate = today.AddDays(-30),
                EndDate = today.AddDays(30),
                Status = ContractStatus.Active,
                ServiceLevel = "Premium"
            };

            var firstRequest = new ServiceRequest
            {
                ServiceRequestId = 1,
                ContractId = firstContract.ContractId,
                Contract = firstContract,
                Description = "Move office equipment",
                CostUsd = 100,
                ExchangeRate = 18.25m,
                Cost = 1825,
                Status = ServiceRequestStatus.Pending,
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            };

            var secondRequest = new ServiceRequest
            {
                ServiceRequestId = 2,
                ContractId = secondContract.ContractId,
                Contract = secondContract,
                Description = "Deliver retail stock",
                CostUsd = 200,
                ExchangeRate = 18.25m,
                Cost = 3650,
                Status = ServiceRequestStatus.Approved,
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            };

            context.Clients.AddRange(firstClient, secondClient);
            context.Contracts.AddRange(firstContract, secondContract);
            context.ServiceRequests.AddRange(firstRequest, secondRequest);

            await context.SaveChangesAsync();

            return context;
        }
    }
}
