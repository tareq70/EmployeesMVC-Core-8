namespace EmployeesMVC_Core_8.Services.Email
{
    public interface IEmailHelper
    {
        Task SendEmailAsync(string to, string subject, string body);

    }
}
