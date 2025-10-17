using System.ComponentModel.DataAnnotations;

namespace Assignment1.Models
{
    public class Message
    {
        public int Id { get; set; }


        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Store who wrote it
        public string? UserId { get; set; }

        public string? UserName { get; set; }

    }
}
