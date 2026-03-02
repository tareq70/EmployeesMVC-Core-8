using EmployeesMVC_Core_8.Enum;
using EmployeesMVC_Core_8.Hubs;
using EmployeesMVC_Core_8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;


namespace EmployeesMVC_Core_8.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<EmployeeHub> _hub;

        public EmployeeController(AppDbContext context, IHubContext<EmployeeHub> hub)
        {
            _context = context;
            _hub = hub;

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
                    e.Latitude,
                    e.Longitude,
                    Status = (int)e.Status,
                    DepartmentName = e.Department.Name
                })
                .ToListAsync();

            return Json(new { data = employees });
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(string name, string email, int departmentId, double latitude, double longitude)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Name and Email are required" });

            var emp = new Employee
            {
                Name = name,
                Email = email,
                Longitude = longitude,
                Latitude = latitude,
                DepartmentId = departmentId
            };

            _context.Employees.Add(emp);
            await _context.SaveChangesAsync();

            await _hub.Clients.All.SendAsync("EmployeeAdded", new
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

    }
}
