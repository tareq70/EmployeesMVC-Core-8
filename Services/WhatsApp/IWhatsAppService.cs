namespace EmployeesMVC_Core_8.Services.WhatsApp
{
    public interface IWhatsAppService
    {
        public Task SendMessage(string phone, string messageText);

    }
}
