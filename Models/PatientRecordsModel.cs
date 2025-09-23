namespace LinkCare_IT15.Models.PatientModel
{
    public class PatientRecordsModel
    {
        public DateTime ConsultationDate { get; set; }
        public string DoctorName { get; set; }
        public string ChiefComplaint { get; set; }
        public string Diagnosis { get; set; }
        public List<string> Prescriptions { get; set; }
        public string BloodPressure { get; set; }
        public int HeartRate { get; set; }
        public double Temperature { get; set; }
        public double Weight { get; set; }
        public string Notes { get; set; }
    }
}
