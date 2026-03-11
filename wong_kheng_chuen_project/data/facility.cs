using System.ComponentModel.DataAnnotations;

namespace wong_kheng_chuen_project.data
{
    public class facility
    {
        [Key]
        public int Booking_ID { get; set; }
        public string? Facility_Description { get; set; }
        public DateTime Booking_Date_From { get; set; }
        public DateTime Booking_Date_To { get; set; }
        public string Booked_By { get; set; }
        public bool Booking_Status { get; set; }
    }
}