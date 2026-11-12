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
    public class ContractsController : ControllerBase
    {
        private readonly IContractService _contractService;

        public ContractsController(
            IContractService contractService)
        {
            _contractService = contractService;
        }

        // GET: /api/contracts
        // GET: /api/contracts?status=Active
        // GET: /api/contracts?serviceLevel=Premium
        // GET: /api/contracts?clientId=1
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Contract>>>
            GetContracts(
                [FromQuery] ContractStatus? status,
                [FromQuery] string? serviceLevel,
                [FromQuery] int? clientId)
        {
            var contracts = await _contractService.GetAllAsync(
                status,
                serviceLevel,
                clientId);

            return Ok(contracts);
        }

        // GET: /api/contracts/1
        [HttpGet("{id:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Contract>>
            GetContract(int id)
        {
            var contract =
                await _contractService.GetByIdAsync(id);

            if (contract is null)
            {
                return NotFound(new
                {
                    message =
                        $"Contract with ID {id} was not found."
                });
            }

            return Ok(contract);
        }

        // POST: /api/contracts
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Contract>>
            CreateContract(CreateContractDto dto)
        {
            var result =
                await _contractService.CreateAsync(dto);

            if (result.Error is not null)
            {
                return BadRequest(new
                {
                    message = result.Error
                });
            }

            return CreatedAtAction(
                nameof(GetContract),
                new { id = result.Contract!.ContractId },
                result.Contract);
        }

        // PATCH: /api/contracts/1/status
        [HttpPatch("{id:int}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateStatus(
            int id,
            UpdateContractStatusDto dto)
        {
            var result =
                await _contractService.UpdateStatusAsync(id, dto);

            if (result.NotFound)
            {
                return NotFound(new
                {
                    message =
                        $"Contract with ID {id} was not found."
                });
            }

            if (result.Error is not null)
            {
                return BadRequest(new
                {
                    message = result.Error
                });
            }

            return Ok(new
            {
                message =
                    "Contract status updated successfully.",
                contractId = id,
                status = dto.Status.ToString()
            });
        }
    }
}
