using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    public class ContractServiceTests
    {
        [Fact]
        public async Task ExistsAsync_ReturnsFalse_WhenContractDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);
            var service = new ContractService(context);

            var result = await service.ExistsAsync(999);

            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_ReturnsTrue_WhenContractExists()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var client = new Client
            {
                Name = "Test Client",
                ContactDetails = "test@example.com",
                Region = "Gauteng"
            };

            var contract = new Contract
            {
                Client = client,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };

            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var service = new ContractService(context);
            var result = await service.ExistsAsync(contract.ContractId);

            Assert.True(result);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsContractWithClient_WhenContractExists()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var client = new Client
            {
                Name = "Test Client",
                ContactDetails = "test@example.com",
                Region = "Gauteng"
            };

            var contract = new Contract
            {
                Client = client,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };

            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var service = new ContractService(context);
            var result = await service.GetByIdAsync(contract.ContractId);

            Assert.NotNull(result);
            Assert.Equal(contract.ContractId, result!.ContractId);
            Assert.Equal("Test Client", result.Client?.Name);
        }

        [Fact]
        public async Task GetByIdAsync_ReturnsNull_WhenContractDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);
            var service = new ContractService(context);

            var result = await service.GetByIdAsync(999);

            Assert.Null(result);
        }

        [Fact]
        public async Task CreateAsync_SavesContract()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var client = new Client
            {
                Name = "Test Client",
                ContactDetails = "test@example.com",
                Region = "Gauteng"
            };

            var contract = new Contract
            {
                Client = client,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };

            var service = new ContractService(context);
            await service.CreateAsync(contract);

            Assert.True(contract.ContractId > 0);
            Assert.Equal(1, await context.Contracts.CountAsync());
        }

        [Fact]
        public async Task DeleteAsync_ThrowsAndKeepsContract_WhenContractHasServiceRequests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var client = new Client
            {
                Name = "Test Client",
                ContactDetails = "test@example.com",
                Region = "Gauteng"
            };

            var contract = new Contract
            {
                Client = client,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };

            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var request = new ServiceRequest
            {
                ContractId = contract.ContractId,
                Description = "Routine maintenance",
                CostUsd = 100m,
                ExchangeRate = 18m,
                Cost = 1800m,
                Status = ServiceRequestStatus.Pending
            };

            context.ServiceRequests.Add(request);
            await context.SaveChangesAsync();

            var service = new ContractService(context);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.DeleteAsync(contract.ContractId));

            Assert.Equal(
                "A contract with service requests cannot be deleted.",
                exception.Message);

            Assert.True(await context.Contracts.AnyAsync(
                item => item.ContractId == contract.ContractId));
        }

        [Fact]
        public async Task DeleteAsync_DeletesContract_WhenItHasNoServiceRequests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var client = new Client
            {
                Name = "Test Client",
                ContactDetails = "test@example.com",
                Region = "Gauteng"
            };

            var contract = new Contract
            {
                Client = client,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };

            context.Contracts.Add(contract);
            await context.SaveChangesAsync();

            var service = new ContractService(context);

            await service.DeleteAsync(contract.ContractId);

            Assert.False(await context.Contracts.AnyAsync(
                item => item.ContractId == contract.ContractId));
        }

        [Fact]
        public async Task DeleteAsync_DoesNothing_WhenContractDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);
            var service = new ContractService(context);

            await service.DeleteAsync(999);

            Assert.Empty(await context.Contracts.ToListAsync());
        }

        [Fact]
        public async Task UpdateAsync_SavesContractChanges()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            int contractId;

            await using (var setupContext = new ApplicationDbContext(options))
            {
                var client = new Client
                {
                    Name = "Test Client",
                    ContactDetails = "test@example.com",
                    Region = "Gauteng"
                };

                var contract = new Contract
                {
                    Client = client,
                    StartDate = new DateTime(2026, 1, 1),
                    EndDate = new DateTime(2026, 12, 31),
                    Status = ContractStatus.Active,
                    ServiceLevel = "Standard"
                };

                setupContext.Contracts.Add(contract);
                await setupContext.SaveChangesAsync();

                contractId = contract.ContractId;
            }

            await using (var updateContext = new ApplicationDbContext(options))
            {
                var contract = await updateContext.Contracts
                    .FirstAsync(item => item.ContractId == contractId);

                contract.Status = ContractStatus.OnHold;
                contract.ServiceLevel = "Premium";

                var service = new ContractService(updateContext);
                await service.UpdateAsync(contract);
            }

            await using (var verificationContext =
                new ApplicationDbContext(options))
            {
                var updatedContract = await verificationContext.Contracts
                    .FirstAsync(item => item.ContractId == contractId);

                Assert.Equal(ContractStatus.OnHold, updatedContract.Status);
                Assert.Equal("Premium", updatedContract.ServiceLevel);
            }
        }

        [Fact]
        public async Task GetAllAsync_FiltersContractsAndOrdersByStartDateDescending()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var firstClient = new Client
            {
                Name = "First Test Client",
                ContactDetails = "first@example.com",
                Region = "Gauteng"
            };

            var secondClient = new Client
            {
                Name = "Second Test Client",
                ContactDetails = "second@example.com",
                Region = "Western Cape"
            };

            var januaryContract = new Contract
            {
                Client = firstClient,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 5, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };

            var marchContract = new Contract
            {
                Client = firstClient,
                StartDate = new DateTime(2026, 3, 1),
                EndDate = new DateTime(2026, 9, 30),
                Status = ContractStatus.Active,
                ServiceLevel = "Premium"
            };

            var onHoldContract = new Contract
            {
                Client = firstClient,
                StartDate = new DateTime(2026, 6, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.OnHold,
                ServiceLevel = "Standard"
            };

            var otherClientContract = new Contract
            {
                Client = secondClient,
                StartDate = new DateTime(2026, 5, 1),
                EndDate = new DateTime(2026, 8, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };

            context.Contracts.AddRange(
                januaryContract,
                marchContract,
                onHoldContract,
                otherClientContract);

            await context.SaveChangesAsync();

            var service = new ContractService(context);

            var results = await service.GetAllAsync(
                status: ContractStatus.Active,
                clientId: firstClient.ClientId,
                startDate: new DateTime(2026, 1, 1),
                endDate: new DateTime(2026, 9, 30));

            Assert.Equal(2, results.Count);
            Assert.Equal(marchContract.ContractId, results[0].ContractId);
            Assert.Equal(januaryContract.ContractId, results[1].ContractId);
            Assert.All(results, contract =>
                Assert.Equal(ContractStatus.Active, contract.Status));
            Assert.All(results, contract =>
                Assert.Equal(firstClient.ClientId, contract.ClientId));
        }
    }
}