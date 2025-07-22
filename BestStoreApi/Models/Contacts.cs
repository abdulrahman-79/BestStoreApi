using System.ComponentModel.DataAnnotations;

namespace BestStoreApi.Models
{
    public class Contacts
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string FirstName { get; set; } = "";
        [MaxLength(100)]
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public required Subject Subject { get; set; }
        public string Message { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
