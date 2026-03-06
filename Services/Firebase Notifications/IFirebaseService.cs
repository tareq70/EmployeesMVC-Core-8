namespace EmployeesMVC_Core_8.Services.Firebase_Notifications
{
    public interface IFirebaseService
    {
        public Task SendNotification(string token);

    }
}
