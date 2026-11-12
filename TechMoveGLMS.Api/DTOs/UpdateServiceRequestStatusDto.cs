using System.ComponentModel.DataAnnotations;
using TechMoveGLMS.Api.Models;

namespace TechMoveGLMS.Api.DTOs
{
    public class UpdateServiceRequestStatusDto
    {
        [Required]
        [EnumDataType(typeof(ServiceRequestStatus))]
        public ServiceRequestStatus Status { get; set; }
    }
}
