using LinkCare_IT15.Models.Entities;
using LinkCare_IT15.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

public class Consultation
{
    [Key]
    public int Id { get; set; }

    [ForeignKey("Doctor")]
    public string DoctorId { get; set; }
    public ApplicationUser Doctor { get; set; }

    [ForeignKey("Patient")]
    public string? PatientId { get; set; }
    public ApplicationUser? Patient { get; set; }

    [StringLength(255)]
    public string? WalkInName { get; set; }

    [ForeignKey("Appointment")]
    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    [Required]
    public DateTime Date { get; set; } = DateTime.Now;

    [Required(ErrorMessage = "Chief Complaint is required")]
    [StringLength(500)]
    public string ChiefComplaint { get; set; } = string.Empty;

    [Required(ErrorMessage = "Diagnosis is required")]
    [StringLength(500)]
    public string Diagnosis { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? Prescriptions { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(20)]
    public string? BloodPressure { get; set; }

    [StringLength(20)]
    public string? HeartRate { get; set; }

    [StringLength(20)]
    public string? Temperature { get; set; }

    [StringLength(20)]
    public string? Weight { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Required(ErrorMessage = "Consultation Fee is required")]
    public decimal ConsultationFee { get; set; }


    public bool IsArchived { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
}
