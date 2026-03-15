using EmployeesMVC_Core_8.Enum;
using EmployeesMVC_Core_8.Hangfire;
using EmployeesMVC_Core_8.Hubs;
using SharedModels.Models;
using EmployeesMVC_Core_8.Services.Email;
using EmployeesMVC_Core_8.Services.Firebase_Notifications;
using EmployeesMVC_Core_8.Services.WhatsApp;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SharedModels.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<EmployeeHub> _hub;
        private readonly IEmailHelper _email;
        private readonly IFirebaseService _firebaseService;
        private readonly IWhatsAppService _whatsappService;


        public EmployeeController(AppDbContext context, IHubContext<EmployeeHub> hub, IEmailHelper email, IFirebaseService firebaseService, IWhatsAppService whatsappService)
        {
            _context = context;
            _hub = hub;
            _email = email;
            _firebaseService = firebaseService;
            _whatsappService = whatsappService;
        }
        public IActionResult Index()
        {
            ViewBag.Departments = _context.Departments.ToList();
            return View();
        }
        // DataTables AJAX
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.Employees
                .Include(e => e.Department)
                .Select(e => new
                {
                    e.EmployeeId, 
                    e.Name,
                    e.Email,
                    e.PhoneNumber,
                    e.Latitude,
                    e.Longitude,
                    Status = (int)e.Status,
                    DepartmentName = e.Department.Name
                })
                .ToListAsync();

            return Json(new { data = employees });
        }
        [HttpPost]
        public async Task<IActionResult> AddEmployee(string name, string email,string phone ,int departmentId, double latitude, double longitude)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Name and Email are required" });

            var emp = new Employee
            {
                Name = name,
                Email = email,
                PhoneNumber = phone,
                Longitude = longitude,
                Latitude = latitude,
                DepartmentId = departmentId
            };

            _context.Employees.Add(emp);

            var saved = await _context.SaveChangesAsync();

            await _whatsappService.SendMessage(emp.PhoneNumber, $"Welcome {name} to ALBAIT Company! We're glad to have you on board.");
            await _hub.Clients.All.SendAsync("EmployeeAdded", new
            {
                emp.EmployeeId,
                emp.Name,
                emp.Email,
                emp.PhoneNumber,
                DepartmentName = _context.Departments.FirstOrDefault(d => d.DepartmentId == emp.DepartmentId)?.Name,
                emp.Status,
                emp.Latitude,
                emp.Longitude
            });

            if (saved > 0)
            {
                await _email.SendEmailAsync(
                    emp.Email,
                    "Welcome to the ALBAIT Company",
                    $"<h2>Welcome {emp.Name}</h2>"
                );
            }

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> EditEmployee(int id, string? name, string? email, int? departmentId, double latitude, double longitude)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null)
                return Json(new { success = false, message = "Employee not found" });

            if (!string.IsNullOrEmpty(name))
                emp.Name = name;

            if (!string.IsNullOrEmpty(email))
                emp.Email = email;

            if (departmentId.HasValue)
                emp.DepartmentId = departmentId.Value;

            if (latitude != 0)
                emp.Latitude = latitude;
            if (longitude != 0)
                emp.Longitude = longitude;

            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("EmployeeUpdated", new
            {
                emp.EmployeeId,
                emp.Name,
                emp.Email,
                DepartmentName = _context.Departments.FirstOrDefault(d => d.DepartmentId == emp.DepartmentId)?.Name,
                emp.Status,
                emp.Latitude,
                emp.Longitude
            });
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return Json(new { success = false, message = "Employee not found" });
            emp.Status = EmployeeStatus.Deleted;

            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("EmployeeDeleted", new { EmployeeId = id });

            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> ToggleEmployeeStatus(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return Json(new { success = false, message = "Employee not found" });

            if (emp.Status == EmployeeStatus.Active)
                emp.Status = EmployeeStatus.Blocked;
            else if (emp.Status == EmployeeStatus.Blocked)
                emp.Status = EmployeeStatus.Active;

            await _context.SaveChangesAsync();
            await _hub.Clients.All.SendAsync("EmployeeStatusToggled", new
            {
                EmployeeId = emp.EmployeeId,
                Status = (int)emp.Status
            });
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> SaveToken([FromBody] string token)
        {
            if (string.IsNullOrEmpty(token))
                return BadRequest();

            var exists = await _context.DeviceTokens.AnyAsync(x => x.Token == token);

            if (!exists)
            {
                _context.DeviceTokens.Add(new DeviceToken
                {
                    Token = token
                });

                await _context.SaveChangesAsync();
            }

            return Ok();
        }
        public async Task<IActionResult> SendTest()
        {
            var deviceTokens = await _context.DeviceTokens.Select(x => x.Token).ToListAsync();
            if (!deviceTokens.Any())
                return Json(new { status = "No Tokens", response = "No FCM tokens found" });

            GoogleCredential credential;
            using (var stream = new FileStream(@"wwwroot/firebase/service-account.json", FileMode.Open, FileAccess.Read))
            {
                credential = GoogleCredential.FromStream(stream)
                    .CreateScoped("https://www.googleapis.com/auth/firebase.messaging");
            }

            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            foreach (var token in deviceTokens)
            {
                var message = new
                {
                    message = new
                    {
                        token = token,
                        notification = new
                        {
                            title = "Hi Tarek",
                            body = "Hello from ALBait Software House 🔥"
                        },
                        webpush = new
                        {
                            notification = new
                            {
                                icon = "https://cdn-icons-png.flaticon.com/512/1827/1827392.png"
                            }
                        }
                    }
                };

                var content = new StringContent(JsonSerializer.Serialize(message), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("https://fcm.googleapis.com/v1/projects/albait-55f36/messages:send", content);
                Console.WriteLine(await response.Content.ReadAsStringAsync());
            }

            return Json(new { status = "Done", response = "Notifications sent" });
        }
    }
}
