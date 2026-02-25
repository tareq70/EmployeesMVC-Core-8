using EmployeesMVC_Core_8.DTOs;

namespace EmployeesMVC_Core_8.Interfaces
{
    public interface IPaymentService
    {
        Task<FawryPaymentResult> CreateFawryPayment(int orderId);

    }
}
