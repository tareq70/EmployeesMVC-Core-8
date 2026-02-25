namespace EmployeesMVC_Core_8.DTOs
{
    public class FawryWebhookDto
    {
        public string referenceNumber { get; set; } = string.Empty; 
        public string merchantRefNumber { get; set; } = string.Empty;
        public string orderStatus { get; set; } = string.Empty;
        public decimal paymentAmount { get; set; }  
    }
}
