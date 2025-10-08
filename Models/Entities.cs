using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using LinkCare_IT15.Models;



namespace LinkCare_IT15.Models.Entities
{
    public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Address { get; set; }
    public string Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }


        // Navigation properties
    public ICollection<Consultation> ConsultationsAsDoctor { get; set; } = new List<Consultation>();
    public ICollection<Consultation> ConsultationsAsPatient { get; set; } = new List<Consultation>();

    // Computed property
    public string FullName => $"{FirstName} {LastName}";
}
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }// PK for this table

        [Required]
        [ForeignKey("User")]
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        [Required]
        [MaxLength(100)]
        public string Specialty { get; set; }

        [Required]
        [MaxLength(50)]

        public string Phone { get; set; }
        public string LicenseNumber { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime Registered { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }
    }


}
