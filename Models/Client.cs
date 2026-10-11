using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace TechMoveGLMS.Web.Models
{
    public class Client
    {
        public int ClientId { get; set; }

        [Required(ErrorMessage = "Client name is required.")]
        [StringLength(100)]
        [Display(Name = "Client Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact details are required.")]
        [StringLength(150)]
        [Display(Name = "Contact Details")]
        public string ContactDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Region is required.")]
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;

        public ICollection<Contract> Contracts { get; set; } = new List<Contract>();
    }
}
