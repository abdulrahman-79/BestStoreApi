using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Models
{
    public class ContactDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = "";
        [Required, MaxLength(100)]
        public string LastName { get; set; } = "";
        [Required, EmailAddress]
        public string Email { get; set; } = "";
        public string? Phone { get; set; } = "";
        public int SubjectId { get; set; }
        [Required]
        public string Message { get; set; } = "";
    }
}
