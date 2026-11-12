using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechMoveGLMS.Api.DTOs;
using TechMoveGLMS.Api.Models;
using TechMoveGLMS.Api.Services;

namespace TechMoveGLMS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clientService;

        public ClientsController(IClientService clientService)
        {
            _clientService = clientService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] string? region = null)
        {
            IEnumerable<Client> clients =
                await _clientService.GetAllAsync(search, region);

            return Ok(clients);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Client>> GetById(int id)
        {
            Client? client =
                await _clientService.GetByIdAsync(id);

            if (client is null)
            {
                return NotFound(new
                {
                    message =
                        $"Client with ID {id} was not found."
                });
            }

            return Ok(client);
        }

        [HttpPost]
        public async Task<ActionResult<Client>> Create(
            CreateClientDto dto)
        {
            Client client = new()
            {
                Name = dto.Name.Trim(),
                ContactDetails = dto.ContactDetails.Trim(),
                Region = dto.Region.Trim()
            };

            Client createdClient =
                await _clientService.CreateAsync(client);

            return CreatedAtAction(
                nameof(GetById),
                new { id = createdClient.ClientId },
                createdClient);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            CreateClientDto dto)
        {
            Client client = new()
            {
                Name = dto.Name.Trim(),
                ContactDetails = dto.ContactDetails.Trim(),
                Region = dto.Region.Trim()
            };

            bool updated =
                await _clientService.UpdateAsync(id, client);

            if (!updated)
            {
                return NotFound(new
                {
                    message =
                        $"Client with ID {id} was not found."
                });
            }

            return Ok(new
            {
                message = "Client updated successfully."
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted =
                await _clientService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message =
                        $"Client with ID {id} was not found."
                });
            }

            return Ok(new
            {
                message = "Client deleted successfully."
            });
        }
    }
}
