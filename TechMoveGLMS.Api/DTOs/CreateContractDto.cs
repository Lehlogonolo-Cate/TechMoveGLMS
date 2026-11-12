using System.ComponentModel.DataAnnotations;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.DTOs
{
    public class CreateContractDto
    {
        [Required]
        public int ClientId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; }

        [Required(ErrorMessage = "Service level is required.")]
        [StringLength(100)]
        public string ServiceLevel { get; set; } = string.Empty;
    }
}
