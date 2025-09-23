namespace LinkCare_IT15.Models.PatientModel
{
    public class PatientAppointmentsModel
    {
        public string PatientName { get; set; }
        public string DoctorName { get; set; }
        public DateTime Start { get; set; }
        public string Type { get; set; }
        public string Status { get; set; }
    }
}
