namespace EmployeesMVC_Core_8.Models
{
    public class Payment
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order? Order { get; set; }

        public decimal Amount { get; set; }

        public string Provider { get; set; } = "Fawry";

        public string? ReferenceNumber { get; set; }

        public string MerchantRef { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        public string? PaymentUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
