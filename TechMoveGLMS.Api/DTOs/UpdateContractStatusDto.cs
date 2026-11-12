using System.ComponentModel.DataAnnotations;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.DTOs
{
    public class UpdateContractStatusDto
    {
        [Required]
        public ContractStatus Status { get; set; }
    }
}