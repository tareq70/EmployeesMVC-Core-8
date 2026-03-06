namespace EmployeesMVC_Core_8.Models
{
    public class DeviceToken
    {
        public int Id { get; set; }

        public string Token { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}