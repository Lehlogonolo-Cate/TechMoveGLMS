using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using TechMoveGLMS.Web.Controllers;
using TechMoveGLMS.Web.Data;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;
using Xunit;

namespace TechMoveGLMS.Tests
{
    public class ContractFileValidationTests
    {
        [Theory]
        [InlineData("agreement.exe", "application/octet-stream")]
        [InlineData("agreement.pdf", "text/plain")]
        public async Task Create_RejectsFile_WhenItIsNotAValidPdf(
            string fileName,
            string contentType)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var controller = CreateController(context);

            var (file, stream) = CreateFile(
                Encoding.UTF8.GetBytes("not a valid PDF"),
                fileName,
                contentType);

            using (stream)
            {
                var result = await controller.Create(
                    CreateValidContract(),
                    file);

                Assert.IsType<ViewResult>(result);

                var error = Assert.Single(
                    controller.ModelState["agreementFile"]!.Errors);

                Assert.Equal(
                    "Only PDF agreement files are allowed.",
                    error.ErrorMessage);
            }

            Assert.Empty(await context.Contracts.ToListAsync());
        }

        [Fact]
        public async Task Create_RejectsFile_WhenPdfIsEmpty()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var controller = CreateController(context);

            var (file, stream) = CreateFile(
                Array.Empty<byte>(),
                "agreement.pdf",
                "application/pdf");

            using (stream)
            {
                var result = await controller.Create(
                    CreateValidContract(),
                    file);

                Assert.IsType<ViewResult>(result);

                var error = Assert.Single(
                    controller.ModelState["agreementFile"]!.Errors);

                Assert.Equal(
                    "The selected PDF file is empty.",
                    error.ErrorMessage);
            }

            Assert.Empty(await context.Contracts.ToListAsync());
        }

        [Fact]
        public async Task Create_RejectsFile_WhenPdfIsLargerThanFiveMegabytes()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            await using var context = new ApplicationDbContext(options);

            var controller = CreateController(context);

            byte[] oversizedContent = new byte[(5 * 1024 * 1024) + 1];

            var (file, stream) = CreateFile(
                oversizedContent,
                "agreement.pdf",
                "application/pdf");

            using (stream)
            {
                var result = await controller.Create(
                    CreateValidContract(),
                    file);

                Assert.IsType<ViewResult>(result);

                var error = Assert.Single(
                    controller.ModelState["agreementFile"]!.Errors);

                Assert.Equal(
                    "The PDF agreement must not exceed 5 MB.",
                    error.ErrorMessage);
            }

            Assert.Empty(await context.Contracts.ToListAsync());
        }

        private static ContractsController CreateController(
            ApplicationDbContext context)
        {
            var contractService = new ContractService(context);
            var clientService = new ClientService(context);
            var environment = new TestWebHostEnvironment();

            return new ContractsController(
                contractService,
                clientService,
                environment);
        }

        private static Contract CreateValidContract()
        {
            return new Contract
            {
                ClientId = 1,
                StartDate = new DateTime(2026, 1, 1),
                EndDate = new DateTime(2026, 12, 31),
                Status = ContractStatus.Active,
                ServiceLevel = "Standard"
            };
        }

        private static (IFormFile File, MemoryStream Stream) CreateFile(
            byte[] content,
            string fileName,
            string contentType)
        {
            var stream = new MemoryStream(content);

            var file = new FormFile(
                stream,
                0,
                stream.Length,
                "agreementFile",
                fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return (file, stream);
        }

        private sealed class TestWebHostEnvironment : IWebHostEnvironment
        {
            public string ApplicationName { get; set; } =
                "TechMoveGLMS.Tests";

            public IFileProvider WebRootFileProvider { get; set; } = null!;

            public string WebRootPath { get; set; } =
                Path.GetTempPath();

            public string EnvironmentName { get; set; } =
                "Development";

            public string ContentRootPath { get; set; } =
                Directory.GetCurrentDirectory();

            public IFileProvider ContentRootFileProvider { get; set; } =
                null!;
        }
    }
}
