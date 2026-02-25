namespace EmployeesMVC_Core_8.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public string? MerchantRef { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
