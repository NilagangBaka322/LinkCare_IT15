using System.ComponentModel.DataAnnotations;

namespace LinkCare_IT15.Models
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; }

        public string? Description { get; set; }

        public int DurationMinutes { get; set; } = 30;

        [MaxLength(255)]
        public string? ImagePath { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
