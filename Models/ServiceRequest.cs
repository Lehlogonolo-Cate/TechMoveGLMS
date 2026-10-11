using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechMoveGLMS.Web.Models
{
    public enum ServiceRequestStatus
    {
        Pending,
        Approved,
        InProgress,
        Completed,
        Cancelled
    }

    public class ServiceRequest
    {
        public int ServiceRequestId { get; set; }

        [Required]
        [Display(Name = "Contract")]
        public int ContractId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Range(0.01, 100000000)]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost in USD")]
        public decimal CostUsd { get; set; }

        [Column(TypeName = "decimal(18,4)")]
        [Display(Name = "USD/ZAR Exchange Rate")]
        public decimal ExchangeRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Cost in ZAR")]
        public decimal Cost { get; set; }

        [Required]
        public ServiceRequestStatus Status { get; set; }
            = ServiceRequestStatus.Pending;

        [Display(Name = "Date Created")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Contract? Contract { get; set; }
    }
}