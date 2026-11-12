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
    public class ServiceRequestsController : ControllerBase
    {
        private readonly IServiceRequestService _service;

        public ServiceRequestsController(
            IServiceRequestService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<
            IEnumerable<ServiceRequest>>> GetAll(
            [FromQuery] string? search = null,
            [FromQuery] int? contractId = null,
            [FromQuery] ServiceRequestStatus? status = null)
        {
            IEnumerable<ServiceRequest> requests =
                await _service.GetAllAsync(
                    search,
                    contractId,
                    status);

            return Ok(requests);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ServiceRequest>> GetById(
            int id)
        {
            ServiceRequest? request =
                await _service.GetByIdAsync(id);

            if (request is null)
            {
                return NotFound(new
                {
                    message =
                        $"Service request with ID {id} was not found."
                });
            }

            return Ok(request);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceRequest>> Create(
            CreateServiceRequestDto dto)
        {
            bool contractExists =
                await _service.ContractExistsAsync(dto.ContractId);

            if (!contractExists)
            {
                return BadRequest(new
                {
                    message =
                        $"Contract with ID {dto.ContractId} does not exist."
                });
            }

            ServiceRequest request = new()
            {
                ContractId = dto.ContractId,
                Description = dto.Description.Trim(),
                CostUsd = dto.CostUsd,
                ExchangeRate = dto.ExchangeRate,
                Status = dto.Status
            };

            ServiceRequest? createdRequest =
                await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new
                {
                    id = createdRequest!.ServiceRequestId
                },
                createdRequest);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(
            int id,
            CreateServiceRequestDto dto)
        {
            ServiceRequest? existingRequest =
                await _service.GetByIdAsync(id);

            if (existingRequest is null)
            {
                return NotFound(new
                {
                    message =
                        $"Service request with ID {id} was not found."
                });
            }

            bool contractExists =
                await _service.ContractExistsAsync(dto.ContractId);

            if (!contractExists)
            {
                return BadRequest(new
                {
                    message =
                        $"Contract with ID {dto.ContractId} does not exist."
                });
            }

            ServiceRequest request = new()
            {
                ContractId = dto.ContractId,
                Description = dto.Description.Trim(),
                CostUsd = dto.CostUsd,
                ExchangeRate = dto.ExchangeRate,
                Status = dto.Status
            };

            await _service.UpdateAsync(id, request);

            return Ok(new
            {
                message =
                    "Service request updated successfully."
            });
        }

        [HttpPatch("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus(
            int id,
            UpdateServiceRequestStatusDto dto)
        {
            ServiceRequest? request =
                await _service.GetByIdAsync(id);

            if (request is null)
            {
                return NotFound(new
                {
                    message =
                        $"Service request with ID {id} was not found."
                });
            }

            request.Status = dto.Status;

            await _service.UpdateAsync(id, request);

            return Ok(new
            {
                message =
                    "Service request status updated successfully.",
                serviceRequestId = id,
                status = dto.Status.ToString()
            });
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message =
                        $"Service request with ID {id} was not found."
                });
            }

            return Ok(new
            {
                message =
                    "Service request deleted successfully."
            });
        }
    }
}
