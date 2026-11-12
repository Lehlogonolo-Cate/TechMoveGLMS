using System.ComponentModel.DataAnnotations;

namespace TechMoveGLMS.Api.DTOs
{
    public class CreateClientDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string ContactDetails { get; set; } =
            string.Empty;

        [Required]
        [StringLength(100)]
        public string Region { get; set; } = string.Empty;
    }
}
