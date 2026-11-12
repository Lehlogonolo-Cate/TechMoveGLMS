using System.ComponentModel.DataAnnotations;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.DTOs
{
    public class CreateServiceRequestDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int ContractId { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } =
            string.Empty;

        [Required]
        [Range(0.01, 100000000)]
        public decimal CostUsd { get; set; }

        [Required]
        [Range(0.0001, 1000)]
        public decimal ExchangeRate { get; set; }

        [Required]
        public ServiceRequestStatus Status { get; set; }
            = ServiceRequestStatus.Pending;
    }
}
