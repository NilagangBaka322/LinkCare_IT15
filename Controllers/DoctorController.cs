//*DoctorController.cs*
using LinkCare_IT15.Data;                  // ✅ for ApplicationDbContext
using LinkCare_IT15.Models.DoctorModel;
using LinkCare_IT15.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks; // Needed for async

namespace LinkCare_IT15.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;  // ✅ add EF DbContext

        // ✅ inject the DbContext via constructor
        public DoctorController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult RegisterDoctor()
        {
            return View();
        }

        // POST: Doctor/RegisterDoctor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterDoctor(Doctor model)
        {
            if (ModelState.IsValid)
            {
                _context.Doctors.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("DoctorDashboard");
            }
            return View(model);
        }

        public IActionResult DoctorDashboard()
        {
            var model = new DoctorDashboardModel
            {
                DoctorName = "Emily Wilson",
                TodayAppointments = 5,
                PendingConsultations = 2,
                TotalPatients = 48,
                UpcomingAppointments = new List<DoctorAppointmentViewModel>
                {
                    new DoctorAppointmentViewModel {
                        PatientName = "John Doe",
                        Title = "Routine Checkup",
                        Status = "Scheduled"
                    },
                    new DoctorAppointmentViewModel {
                        PatientName = "Jane Smith",
                        Title = "Follow-up",
                        Status = "Scheduled"
                    }
                },
                RecentActivity = new List<ActivityViewModel>
                {
                    new ActivityViewModel { Label="Consultation completed", User="John Doe", Ago=TimeSpan.FromHours(1)},
                    new ActivityViewModel { Label="Prescription added", User="Jane Smith", Ago=TimeSpan.FromHours(3)},
                }
            };

            return View(model);
        }

        public IActionResult DoctorAppointments()
        {
            var model = new DoctorAppointmentsModel
            {
                SelectedDate = DateTime.Today,
                Appointments = new List<DoctorAppointmentViewModel>
                {
                    new DoctorAppointmentViewModel {
                        PatientName = "Patient User",
                        DoctorName = "Dr. Emily Wilson",
                        AppointmentDate = DateTime.Today.AddHours(9),
                        Title = "Routine Checkup",
                        Status = "Scheduled"
                    },
                    new DoctorAppointmentViewModel {
                        PatientName = "Sarah Smith",
                        DoctorName = "Dr. Emily Wilson",
                        AppointmentDate = DateTime.Today.AddHours(10),
                        Title = "Follow-up",
                        Status = "Scheduled"
                    },
                    new DoctorAppointmentViewModel {
                        PatientName = "Michael Johnson",
                        DoctorName = "Dr. Emily Wilson",
                        AppointmentDate = DateTime.Today.AddHours(11),
                        Title = "Consultation",
                        Status = "Completed"
                    },
                    new DoctorAppointmentViewModel {
                        PatientName = "Lisa Rodriguez",
                        DoctorName = "Dr. Emily Wilson",
                        AppointmentDate = DateTime.Today.AddHours(13).AddMinutes(30),
                        Title = "Vaccination",
                        Status = "Scheduled"
                    },
                    new DoctorAppointmentViewModel {
                        PatientName = "David Chen",
                        DoctorName = "Dr. Emily Wilson",
                        AppointmentDate = DateTime.Today.AddHours(14).AddMinutes(30),
                        Title = "Emergency",
                        Status = "Scheduled"
                    }
                }
            };

            return View(model);
        }

        // ✅ Corrected DoctorConsultation method
        public IActionResult DoctorConsultation()
        {
            var consultation = _context.Consultations
                .Include(c => c.Patient) // Include the Patient entity  
                .Include(c => c.Doctor)  // Include the Doctor entity  
                .Select(c => new ConsultationViewModel
                {
                    ConsultationId = c.ConsultationId,
                    Date = c.Date,
                    ChiefComplaint = c.ChiefComplaint,
                    Diagnosis = c.Diagnosis,
                    Notes = c.Notes,
                    BloodPressure = c.BloodPressure,
                    HeartRate = c.HeartRate,
                    Temperature = c.Temperature,
                    Weight = c.Weight,
                    PatientName = c.Patient.FirstName + " " + c.Patient.LastName,
                    DoctorName = c.Doctor.FirstName + " " + c.Doctor.LastName
                })
                .ToList();

            return View(consultation);
        }

        // ✅ Add Consultation (POST)
        [HttpPost]
        public IActionResult AddConsultation(ConsultationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var consultation = new Consultations
                {
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    ChiefComplaint = model.ChiefComplaint,
                    Diagnosis = model.Diagnosis,
                    Prescription = string.Join("\n", model.Prescriptions ?? new List<string>()),
                    Notes = model.Notes,
                    BloodPressure = model.BloodPressure,
                    HeartRate = model.HeartRate,
                    Temperature = model.Temperature,
                    Weight = model.Weight,
                    Date = DateTime.Now
                };

                _context.Consultations.Add(consultation);
                _context.SaveChanges();

                return RedirectToAction("DoctorConsultation");
            }

            return View(model);
        }

        [HttpPost]
        public IActionResult RegisterPatient(ConsultationViewModel model)
        {
            if (ModelState.IsValid)
            {
                // currently just adds to static list
                registeredPatients.Add(model);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public JsonResult SearchPatients(string query)
        {
            var results = registeredPatients
                .Where(p => (p.FirstName + " " + p.LastName)
                    .Contains(query, StringComparison.OrdinalIgnoreCase))
                .Select(p => new { name = p.FirstName + " " + p.LastName, contact = p.ContactNumber })
                .ToList();

            return Json(results);
        }

        public IActionResult DoctorMedicalRecords()
        {
            var records = new List<ConsultationViewModel>
            {
                new ConsultationViewModel {
                    PatientName = "Patient User",
                    DoctorName = "Dr. Emily Wilson",
                    Date = DateTime.Now.AddDays(-20),
                    ChiefComplaint = "Annual physical examination",
                    Diagnosis = "Hypertension - well controlled, Diabetes Type 2 - stable",
                    Prescriptions = new List<string> { "Metformin 500mg twice daily", "Lisinopril 10mg daily" },
                    BloodPressure = "128/82",
                    HeartRate = "72",
                    Temperature = "37.0",
                    Weight = "82",
                    Notes = "Patient reports good adherence to medications. Blood pressure well controlled."
                },
                new ConsultationViewModel {
                    PatientName = "Sarah Smith",
                    DoctorName = "Dr. Robert Brown",
                    Date = DateTime.Now.AddDays(-25),
                    ChiefComplaint = "Shortness of breath",
                    Diagnosis = "Asthma exacerbation",
                    Prescriptions = new List<string> { "Albuterol as needed", "Prednisone 20mg daily for 5 days" },
                    BloodPressure = "118/75",
                    HeartRate = "88",
                    Temperature = "36.9",
                    Weight = "61",
                    Notes = "Patient experienced increased symptoms during high pollen season."
                }
            };

            return View(records);
        }

        public IActionResult DoctorPatients()
        {
            var model = new DoctorPatientsModel
            {
                Patients = new List<DoctorPatientViewModel>
                {
                    new DoctorPatientViewModel {
                        PatientName = "Patient User",
                        Age = 30,
                        Gender = "Male",
                        Contact = "0912-345-6789",
                        Status = "Active",
                        LastVisit = DateTime.Today.AddDays(-3)
                    },
                    new DoctorPatientViewModel {
                        PatientName = "Sarah Smith",
                        Age = 27,
                        Gender = "Female",
                        Contact = "0917-222-3333",
                        Status = "Active",
                        LastVisit = DateTime.Today.AddDays(-10)
                    },
                    new DoctorPatientViewModel {
                        PatientName = "Michael Johnson",
                        Age = 45,
                        Gender = "Male",
                        Contact = "0918-555-9999",
                        Status = "Inactive",
                        LastVisit = DateTime.Today.AddDays(-60)
                    },
                    new DoctorPatientViewModel {
                        PatientName = "Lisa Rodriguez",
                        Age = 35,
                        Gender = "Female",
                        Contact = "0917-123-4567",
                        Status = "Active",
                        LastVisit = DateTime.Today.AddDays(-1)
                    }
                }
            };

            return View(model);
        }

        // static lists (used for demo only)
        private static List<ConsultationViewModel> registeredPatients = new List<ConsultationViewModel>();

        // =========================
        // API: Create Appointment
        // =========================
        public class CreateAppointmentDto
        {
            public string PatientName { get; set; }
            public string Title { get; set; }
            public string StartDate { get; set; } // ISO string
            public string EndDate { get; set; }   // ISO string
        }

        [HttpPost]
        [Route("Doctor/CreateAppointment")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            // Validate input
            if (dto == null || string.IsNullOrWhiteSpace(dto.PatientName) || string.IsNullOrWhiteSpace(dto.Title)
                || string.IsNullOrWhiteSpace(dto.StartDate) || string.IsNullOrWhiteSpace(dto.EndDate))
            {
                return BadRequest("Invalid appointment data.");
            }

            // Parse dates
            if (!DateTime.TryParse(dto.StartDate, out var startDate) ||
                !DateTime.TryParse(dto.EndDate, out var endDate))
            {
                return BadRequest("Invalid date format.");
            }

            // TODO: Save to database if you have an Appointment entity
            // For demo, just return the appointment info

            // You can add DB save logic here if you have a persistent model

            // Return appointment info for FullCalendar
            return Json(new
            {
                id = Guid.NewGuid().ToString(), // Replace with DB-generated ID if available
                title = $"{dto.Title}: {dto.PatientName}",
                startDate = startDate.ToString("s"),
                endDate = endDate.ToString("s"),
                patientName = dto.PatientName
            });
        }
        // =========================
        // END API: Create Appointment
        // =========================

    }
}

