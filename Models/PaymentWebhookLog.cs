namespace EmployeesMVC_Core_8.Models
{
    public class PaymentWebhookLog
    {
        public int Id { get; set; }

        public string? ReferenceNumber { get; set; }

        public string? MerchantRefNumber { get; set; }

        public string? Status { get; set; }

        public string? RawResponse { get; set; }

        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;
    }
}
