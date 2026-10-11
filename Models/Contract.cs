using System.ComponentModel.DataAnnotations;

namespace TechMoveGLMS.Web.Models
{
    public enum ContractStatus
    {
        Draft,
        Active,
        Expired,
        OnHold
    }

    public class Contract
    {
        public int ContractId { get; set; }

        [Required]
        [Display(Name = "Client")]
        public int ClientId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public ContractStatus Status { get; set; }

        [Required(ErrorMessage = "Service level is required.")]
        [StringLength(100)]
        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; } = string.Empty;

        [Display(Name = "Signed Agreement")]
        public string? AgreementFileName { get; set; }

        public string? AgreementStoredFileName { get; set; }

        public Client? Client { get; set; }

        public ICollection<ServiceRequest> ServiceRequests { get; set; }
            = new List<ServiceRequest>();
    }
}