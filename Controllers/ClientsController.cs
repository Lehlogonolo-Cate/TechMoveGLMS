using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Web.Models;
using TechMoveGLMS.Web.Services;

namespace TechMoveGLMS.Web.Controllers
{
    public class ClientsController : Controller
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        // GET: Clients
        public async Task<IActionResult> Index()
        {
            var clients = await _clientService.GetAllAsync();
            return View(clients);
        }

        // GET: Clients/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _clientService.GetByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // GET: Clients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Name,ContactDetails,Region")] Client client)
        {
            if (!ModelState.IsValid)
            {
                return View(client);
            }

            await _clientService.CreateAsync(client);

            TempData["SuccessMessage"] =
                "Client created successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _clientService.GetByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("ClientId,Name,ContactDetails,Region")] Client client)
        {
            if (id != client.ClientId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(client);
            }

            var updated = await _clientService.UpdateAsync(client);

            if (!updated)
            {
                return NotFound();
            }

            TempData["SuccessMessage"] =
                "Client updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        // GET: Clients/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var client = await _clientService.GetByIdAsync(id.Value);

            if (client == null)
            {
                return NotFound();
            }

            return View(client);
        }

        // POST: Clients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var deleted = await _clientService.DeleteAsync(id);

            if (!deleted)
            {
                TempData["ErrorMessage"] =
                    "The client could not be deleted. Remove its contracts first.";

                return RedirectToAction(nameof(Index));
            }

            TempData["SuccessMessage"] =
                "Client deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}