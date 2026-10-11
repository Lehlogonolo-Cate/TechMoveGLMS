using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;

namespace TechMoveGLMS.Web.Controllers
{
    public class ContractsController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IClientService _clientService;
        private readonly IWebHostEnvironment _environment;

        private const long MaximumFileSize = 5 * 1024 * 1024;

        public ContractsController(
            IContractService contractService,
            IClientService clientService,
            IWebHostEnvironment environment)
        {
            _contractService = contractService;
            _clientService = clientService;
            _environment = environment;
        }

        public async Task<IActionResult> Index(
            ContractStatus? status,
            int? clientId,
            DateTime? startDate,
            DateTime? endDate)
        {
            List<Contract> contracts =
                await _contractService.GetAllAsync(
                    status,
                    clientId,
                    startDate,
                    endDate);

            await LoadFilterOptionsAsync(clientId);

            ViewBag.SelectedStatus = status;
            ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");

            return View(contracts);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contract? contract =
                await _contractService.GetByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        public async Task<IActionResult> Create()
        {
            await LoadClientsAsync();

            Contract contract = new()
            {
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1),
                Status = ContractStatus.Draft
            };

            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Contract contract,
            IFormFile? agreementFile)
        {
            ValidateContractDates(contract);

            if (agreementFile != null)
            {
                ValidateAgreementFile(agreementFile);
            }

            if (!ModelState.IsValid)
            {
                await LoadClientsAsync(contract.ClientId);
                return View(contract);
            }

            if (agreementFile != null)
            {
                await SaveAgreementFileAsync(contract, agreementFile);
            }

            await _contractService.CreateAsync(contract);

            TempData["SuccessMessage"] =
                "Contract created successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contract? contract =
                await _contractService.GetByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }

            await LoadClientsAsync(contract.ClientId);

            return View(contract);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            Contract contract,
            IFormFile? agreementFile)
        {
            if (id != contract.ContractId)
            {
                return NotFound();
            }

            ValidateContractDates(contract);

            if (agreementFile != null)
            {
                ValidateAgreementFile(agreementFile);
            }

            if (!ModelState.IsValid)
            {
                await LoadClientsAsync(contract.ClientId);
                return View(contract);
            }

            Contract? existingContract =
                await _contractService.GetByIdAsync(id);

            if (existingContract == null)
            {
                return NotFound();
            }

            existingContract.ClientId = contract.ClientId;
            existingContract.StartDate = contract.StartDate;
            existingContract.EndDate = contract.EndDate;
            existingContract.Status = contract.Status;
            existingContract.ServiceLevel = contract.ServiceLevel;

            if (agreementFile != null)
            {
                DeleteAgreementFile(existingContract);
                await SaveAgreementFileAsync(
                    existingContract,
                    agreementFile);
            }

            await _contractService.UpdateAsync(existingContract);

            TempData["SuccessMessage"] =
                "Contract updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Contract? contract =
                await _contractService.GetByIdAsync(id.Value);

            if (contract == null)
            {
                return NotFound();
            }

            return View(contract);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Contract? contract =
                await _contractService.GetByIdAsync(id);

            if (contract == null)
            {
                return NotFound();
            }

            try
            {
                await _contractService.DeleteAsync(id);
                DeleteAgreementFile(contract);

                TempData["SuccessMessage"] =
                    "Contract deleted successfully.";
            }
            catch (InvalidOperationException exception)
            {
                TempData["ErrorMessage"] = exception.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> DownloadAgreement(int id)
        {
            Contract? contract =
                await _contractService.GetByIdAsync(id);

            if (contract == null ||
                string.IsNullOrWhiteSpace(
                    contract.AgreementStoredFileName))
            {
                return NotFound();
            }

            string filePath = Path.Combine(
                GetAgreementFolder(),
                contract.AgreementStoredFileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound();
            }

            byte[] fileBytes =
                await System.IO.File.ReadAllBytesAsync(filePath);

            return File(
                fileBytes,
                "application/pdf",
                contract.AgreementFileName ??
                    "ContractAgreement.pdf");
        }

        private async Task LoadClientsAsync(
            int? selectedClientId = null)
        {
            List<Client> clients =
                await _clientService.GetAllAsync();

            ViewBag.ClientId = new SelectList(
                clients,
                "ClientId",
                "Name",
                selectedClientId);
        }

        private async Task LoadFilterOptionsAsync(
            int? selectedClientId)
        {
            List<Client> clients =
                await _clientService.GetAllAsync();

            ViewBag.Clients = new SelectList(
                clients,
                "ClientId",
                "Name",
                selectedClientId);
        }

        private void ValidateContractDates(Contract contract)
        {
            if (contract.EndDate < contract.StartDate)
            {
                ModelState.AddModelError(
                    nameof(contract.EndDate),
                    "The end date must be on or after the start date.");
            }
        }

        private void ValidateAgreementFile(IFormFile file)
        {
            string extension =
                Path.GetExtension(file.FileName).ToLowerInvariant();

            if (extension != ".pdf" ||
                file.ContentType != "application/pdf")
            {
                ModelState.AddModelError(
                    "agreementFile",
                    "Only PDF agreement files are allowed.");
            }

            if (file.Length == 0)
            {
                ModelState.AddModelError(
                    "agreementFile",
                    "The selected PDF file is empty.");
            }

            if (file.Length > MaximumFileSize)
            {
                ModelState.AddModelError(
                    "agreementFile",
                    "The PDF agreement must not exceed 5 MB.");
            }
        }

        private async Task SaveAgreementFileAsync(
            Contract contract,
            IFormFile agreementFile)
        {
            string agreementFolder = GetAgreementFolder();

            Directory.CreateDirectory(agreementFolder);

            string storedFileName =
                $"{Guid.NewGuid()}.pdf";

            string filePath = Path.Combine(
                agreementFolder,
                storedFileName);

            await using FileStream stream =
                new(filePath, FileMode.Create);

            await agreementFile.CopyToAsync(stream);

            contract.AgreementFileName =
                Path.GetFileName(agreementFile.FileName);

            contract.AgreementStoredFileName =
                storedFileName;
        }

        private string GetAgreementFolder()
        {
            return Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "contracts");
        }

        private void DeleteAgreementFile(Contract contract)
        {
            if (string.IsNullOrWhiteSpace(
                contract.AgreementStoredFileName))
            {
                return;
            }

            string filePath = Path.Combine(
                GetAgreementFolder(),
                contract.AgreementStoredFileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }
    }
}
