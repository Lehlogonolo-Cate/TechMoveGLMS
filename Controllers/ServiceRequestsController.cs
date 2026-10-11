using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;

namespace TechMoveGLMS.Web.Controllers
{
    public class ServiceRequestsController : Controller
    {
        private readonly IServiceRequestService _serviceRequestService;
        private readonly ICurrencyService _currencyService;

        public ServiceRequestsController(
            IServiceRequestService serviceRequestService,
            ICurrencyService currencyService)
        {
            _serviceRequestService = serviceRequestService;
            _currencyService = currencyService;
        }

        public async Task<IActionResult> Index(
            ServiceRequestStatus? status,
            int? contractId,
            string? searchTerm)
        {
            var requests = await _serviceRequestService.GetAllAsync(
                status,
                contractId,
                searchTerm);

            ViewBag.SelectedStatus = status;
            ViewBag.SelectedContractId = contractId;
            ViewBag.SearchTerm = searchTerm;

            await LoadContractsAsync(contractId);

            return View(requests);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest =
                await _serviceRequestService.GetByIdAsync(id.Value);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        public async Task<IActionResult> Create()
        {
            await LoadContractsAsync();

            return View(new ServiceRequest
            {
                Status = ServiceRequestStatus.Pending
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ServiceRequest serviceRequest)
        {
            if (!await _serviceRequestService.IsContractActiveAsync(
                    serviceRequest.ContractId))
            {
                ModelState.AddModelError(
                    nameof(serviceRequest.ContractId),
                    "Please select a valid active contract.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    serviceRequest.ExchangeRate =
                        await _currencyService.GetUsdToZarRateAsync();

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
                catch (Exception)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The exchange rate could not be retrieved. " +
                        "Please check your internet connection and try again.");
                }
            }

            await LoadContractsAsync(serviceRequest.ContractId);
            return View(serviceRequest);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest =
                await _serviceRequestService.GetByIdAsync(id.Value);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            await LoadContractsAsync(serviceRequest.ContractId);

            return View(serviceRequest);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ServiceRequest serviceRequest)
        {
            if (id != serviceRequest.ServiceRequestId)
            {
                return NotFound();
            }

            if (!await _serviceRequestService.IsContractActiveAsync(
                    serviceRequest.ContractId))
            {
                ModelState.AddModelError(
                    nameof(serviceRequest.ContractId),
                    "Please select a valid active contract.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    serviceRequest.ExchangeRate =
                        await _currencyService.GetUsdToZarRateAsync();

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
                catch (Exception)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "The service request could not be updated. " +
                        "Please check your connection and try again.");
                }
            }

            await LoadContractsAsync(serviceRequest.ContractId);
            return View(serviceRequest);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var serviceRequest =
                await _serviceRequestService.GetByIdAsync(id.Value);

            if (serviceRequest == null)
            {
                return NotFound();
            }

            return View(serviceRequest);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (!await _serviceRequestService.ExistsAsync(id))
            {
                return NotFound();
            }

            await _serviceRequestService.DeleteAsync(id);

            TempData["SuccessMessage"] =
                "Service request deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private async Task LoadContractsAsync(
            int? selectedContractId = null)
        {
            var contracts =
                await _serviceRequestService.GetActiveContractsAsync();

            ViewBag.Contracts = contracts.Select(contract =>
                new SelectListItem
                {
                    Value = contract.ContractId.ToString(),
                    Text = $"{contract.Client?.Name} - " +
                           $"{contract.ServiceLevel}",
                    Selected =
                        contract.ContractId == selectedContractId
                }).ToList();
        }
    }
}
