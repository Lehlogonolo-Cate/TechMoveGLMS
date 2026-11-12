using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveGLMS.Web.Filters;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;

namespace TechMoveGLMS.Web.Controllers
{
    [SessionAuthorize]
    public class ServiceRequestsController : Controller
    {
        private readonly IServiceRequestService
            _serviceRequestService;

        private readonly ICurrencyService
            _currencyService;

        public ServiceRequestsController(
            IServiceRequestService serviceRequestService,
            ICurrencyService currencyService)
        {
            _serviceRequestService =
                serviceRequestService;

            _currencyService =
                currencyService;
        }

        public async Task<IActionResult> Index(
            ServiceRequestStatus? status,
            int? contractId,
            string? searchTerm)
        {
            try
            {
                var requests =
                    await _serviceRequestService.GetAllAsync(
                        status,
                        contractId,
                        searchTerm);

                ViewBag.SelectedStatus = status;
                ViewBag.SelectedContractId = contractId;
                ViewBag.SearchTerm = searchTerm;

                await LoadContractsAsync(contractId);

                return View(requests);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The service requests could not be loaded because the API is unavailable.";

                return View(
                    new List<ServiceRequest>());
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var serviceRequest =
                    await _serviceRequestService
                        .GetByIdAsync(id.Value);

                if (serviceRequest == null)
                {
                    return NotFound();
                }

                return View(serviceRequest);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The service request could not be loaded.";

                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Create()
        {
            try
            {
                await LoadContractsAsync();

                return View(new ServiceRequest
                {
                    Status =
                        ServiceRequestStatus.Pending
                });
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "Active contracts could not be loaded.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ServiceRequest serviceRequest)
        {
            try
            {
                var contractIsActive =
                    await _serviceRequestService
                        .IsContractActiveAsync(
                            serviceRequest.ContractId);

                if (!contractIsActive)
                {
                    ModelState.AddModelError(
                        nameof(
                            serviceRequest.ContractId),
                        "Please select a valid active contract.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadContractsAsync(
                        serviceRequest.ContractId);

                    return View(serviceRequest);
                }

                serviceRequest.ExchangeRate =
                    await _currencyService
                        .GetUsdToZarRateAsync();

                serviceRequest.Cost = Math.Round(
                    serviceRequest.CostUsd *
                    serviceRequest.ExchangeRate,
                    2,
                    MidpointRounding.AwayFromZero);

                serviceRequest.Status =
                    ServiceRequestStatus.Pending;

                await _serviceRequestService.CreateAsync(
                    serviceRequest);

                TempData["SuccessMessage"] =
                    "Service request created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    exception.Message);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The service request could not be created because an external service or the API is unavailable.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred while creating the service request.");
            }

            try
            {
                await LoadContractsAsync(
                    serviceRequest.ContractId);
            }
            catch (HttpRequestException)
            {
                ViewBag.Contracts =
                    new List<SelectListItem>();
            }

            return View(serviceRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var serviceRequest =
                    await _serviceRequestService
                        .GetByIdAsync(id.Value);

                if (serviceRequest == null)
                {
                    return NotFound();
                }

                await LoadContractsAsync(
                    serviceRequest.ContractId);

                return View(serviceRequest);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The service request could not be loaded.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ServiceRequest serviceRequest)
        {
            if (id !=
                serviceRequest.ServiceRequestId)
            {
                return NotFound();
            }

            try
            {
                var contractIsActive =
                    await _serviceRequestService
                        .IsContractActiveAsync(
                            serviceRequest.ContractId);

                if (!contractIsActive)
                {
                    ModelState.AddModelError(
                        nameof(
                            serviceRequest.ContractId),
                        "Please select a valid active contract.");
                }

                if (!ModelState.IsValid)
                {
                    await LoadContractsAsync(
                        serviceRequest.ContractId);

                    return View(serviceRequest);
                }

                serviceRequest.ExchangeRate =
                    await _currencyService
                        .GetUsdToZarRateAsync();

                serviceRequest.Cost = Math.Round(
                    serviceRequest.CostUsd *
                    serviceRequest.ExchangeRate,
                    2,
                    MidpointRounding.AwayFromZero);

                await _serviceRequestService.UpdateAsync(
                    serviceRequest);

                TempData["SuccessMessage"] =
                    "Service request updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    exception.Message);
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The service request could not be updated because an external service or the API is unavailable.");
            }
            catch (Exception)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred while updating the service request.");
            }

            try
            {
                await LoadContractsAsync(
                    serviceRequest.ContractId);
            }
            catch (HttpRequestException)
            {
                ViewBag.Contracts =
                    new List<SelectListItem>();
            }

            return View(serviceRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var serviceRequest =
                    await _serviceRequestService
                        .GetByIdAsync(id.Value);

                if (serviceRequest == null)
                {
                    return NotFound();
                }

                return View(serviceRequest);
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The service request could not be loaded.";

                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult>
            DeleteConfirmed(int id)
        {
            try
            {
                if (!await _serviceRequestService
                        .ExistsAsync(id))
                {
                    return NotFound();
                }

                await _serviceRequestService
                    .DeleteAsync(id);

                TempData["SuccessMessage"] =
                    "Service request deleted successfully.";
            }
            catch (HttpRequestException)
            {
                TempData["ErrorMessage"] =
                    "The service request could not be deleted because the API is unavailable.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadContractsAsync(
            int? selectedContractId = null)
        {
            var contracts =
                await _serviceRequestService
                    .GetActiveContractsAsync();

            ViewBag.Contracts = contracts
                .Select(contract =>
                    new SelectListItem
                    {
                        Value =
                            contract.ContractId.ToString(),

                        Text =
                            $"{contract.Client?.Name} - " +
                            $"{contract.ServiceLevel}",

                        Selected =
                            contract.ContractId ==
                            selectedContractId
                    })
                .ToList();
        }
    }
}
