namespace EmployeesMVC_Core_8.DTOs
{
    public class FawryPaymentResult
    {
        public bool Success { get; set; }
        public string PaymentUrl { get; set; }
        public string ReferenceNumber { get; set; }
        public string MerchantRef { get; set; }
    }
}
