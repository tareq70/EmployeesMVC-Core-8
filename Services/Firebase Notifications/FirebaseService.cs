using FirebaseAdmin.Messaging;

namespace EmployeesMVC_Core_8.Services.Firebase_Notifications
{
    public class FirebaseService : IFirebaseService
    {
        public async Task SendNotification(string token)
        {
            var message = new Message()
            {
                Token = token,
                Notification = new Notification()
                {
                    Title = "Test Notification",
                    Body = "Hello from Albait company" 
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}
