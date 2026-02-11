using EmployeesMVC_Core_8.Enum;
using EmployeesMVC_Core_8.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeesMVC_Core_8.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly AppDbContext _context;
        public EmployeeController(AppDbContext context)
        {
            _context = context;
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
                    Status = (int)e.Status,
                    DepartmentName = e.Department.Name
                })
                .ToListAsync();

            return Json(new { data = employees });
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(string name, string email, int departmentId)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
                return Json(new { success = false, message = "Name and Email are required" });

            _context.Employees.Add(new Employee
            {
                Name = name,
                Email = email,
                DepartmentId = departmentId
            });

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> EditEmployee(int id, string? name, string? email, int? departmentId)
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

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var emp = await _context.Employees.FindAsync(id);
            if (emp == null) return Json(new { success = false, message = "Employee not found" });
            emp.Status = EmployeeStatus.Deleted;

            await _context.SaveChangesAsync();
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

            return Json(new { success = true });
        }

    }
}
