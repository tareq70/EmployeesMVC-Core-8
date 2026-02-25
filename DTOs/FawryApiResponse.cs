namespace EmployeesMVC_Core_8.DTOs
{
    public class FawryApiResponse
    {
        public string referenceNumber { get; set; } =string.Empty;
        public string paymentUrl { get; set; } = string.Empty;
        public int statusCode { get; set; } 
        public string statusDescription { get; set; } = string.Empty;
    }
}
